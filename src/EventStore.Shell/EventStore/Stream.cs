using System.Threading.Tasks;
using EventStore.ClientAPI;
using Optional;
using Optional.Async.Extensions;

namespace EventStore.Shell.EventStore
{
    public class Stream
    {
        public static Stream Current { get; private set; }

        public static async Task SetCurrent(string name)
        {
            if (Current?._name == name) return;

            var stream     = new Stream(name);
            var connection = ConnectionManager.GetCurrentConnection();
            var check      = await stream.CheckStream(connection);
            if (check == -1) return;

            Output.WriteInfo($"Selected stream {name}, last event number is {check}");
            Current = stream;
        }

        readonly string _name;

        long _currentPosition;

        public Stream(string name) => _name = name;

        public async Task<ResolvedEvent[]> ReadForward(long start, int count)
        {
            var connection = ConnectionManager.GetCurrentConnection();
            if (await CheckStream(connection) == -1) return default;

            Output.WriteInfo($"Reading the stream{_name} forward from {_currentPosition}");

            var slice = await connection.ReadStreamEventsForwardAsync(_name, start == -1 ? _currentPosition : start, count, true);

            _currentPosition = slice.NextEventNumber;

            return slice.Events;
        }

        public async Task<Option<StreamMetadataResult>> ReadMeta()
        {
            var connection = ConnectionManager.GetCurrentConnection();
            if (await CheckStream(connection) == -1) return default;

            return await connection.GetStreamMetadataAsync(_name).SomeNotNullAsync();
        }

        async Task<StreamInfo> GetInfo(IEventStoreConnection connection)
        {
            var slice = await connection.ReadStreamEventsBackwardAsync(_name, StreamPosition.End, 1, true);

            return slice.Status switch
            {
                SliceReadStatus.Success        => new StreamInfo(slice.LastEventNumber),
                SliceReadStatus.StreamNotFound => StreamInfo.NonExistent,
                SliceReadStatus.StreamDeleted  => StreamInfo.Deleted,
                _                              => StreamInfo.NonExistent
            };
        }

        async Task<long> CheckStream(IEventStoreConnection connection)
        {
            if (connection == null) return -1;

            var streamInfo = await GetInfo(connection);

            if (streamInfo.Exists) return streamInfo.LastEventNumber;

            var message = streamInfo.IsDeleted ? "is a deleted stream" : "does not exist";
            Output.WriteWarning($"Stream {_name} {message}");
            return -1;
        }

        class StreamInfo
        {
            public long LastEventNumber { get; }
            public bool Exists          { get; private set; }
            public bool IsDeleted       { get; private set; }

            public static StreamInfo NonExistent = new StreamInfo {Exists = false};

            public static StreamInfo Deleted = new StreamInfo {Exists = false, IsDeleted = true};

            public StreamInfo(long lastEventNumber)
            {
                Exists          = true;
                IsDeleted       = false;
                LastEventNumber = lastEventNumber;
            }

            StreamInfo() { }
        }
    }
}
