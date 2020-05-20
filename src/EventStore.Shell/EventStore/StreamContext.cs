using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Optional;
using Optional.Async.Extensions;

namespace EventStore.Shell.EventStore
{
    public class StreamContext
    {
        public StreamContext(ConnectionContext connectionContext, string streamName)
        {
            _connectionContext = connectionContext;
            StreamName         = streamName;
        }

        public string StreamName { get; }

        public string Status => $"{StreamName} @ {_currentPosition}:{_lastEventPosition}";

        long _currentPosition;
        long _lastEventPosition;

        readonly ConnectionContext _connectionContext;

        public async Task<(ResolvedEvent[] Events, bool IsEndOfStream)> ReadForward(long start = -1, int count = 10, Action<string> log = default)
        {
            var connection = await _connectionContext.GetCurrentConnection();
            if (!(await GetLastEventNumber(connection)).HasValue) return default;

            log?.Invoke($"Reading the stream {StreamName} forward from {_currentPosition}");

            var slice = await connection.ReadStreamEventsForwardAsync(StreamName, start == -1 ? _currentPosition : start, count, true);

            _currentPosition = slice.NextEventNumber;

            return (slice.Events, slice.IsEndOfStream);
        }

        public async Task<Option<StreamMetadataResult>> ReadMeta()
        {
            var connection = await _connectionContext.GetCurrentConnection();

            var x = await GetLastEventNumber(connection);

            return await x.MatchAsync(
                _ => connection.GetStreamMetadataAsync(StreamName).SomeNotNullAsync(),
                () => null
            );
        }

        async Task<StreamInfo> GetInfo(IEventStoreConnection connection)
        {
            var slice = await connection.ReadStreamEventsBackwardAsync(StreamName, StreamPosition.End, 1, true);

            return slice.Status switch
            {
                SliceReadStatus.Success        => new StreamInfo(slice.LastEventNumber),
                SliceReadStatus.StreamNotFound => StreamInfo.NonExistent,
                SliceReadStatus.StreamDeleted  => StreamInfo.Deleted,
                _                              => StreamInfo.NonExistent
            };
        }

        public async Task<Option<long>> GetLastEventNumber(IEventStoreConnection connection)
        {
            if (connection == null) return 1L.None();

            var streamInfo = await GetInfo(connection);
            _lastEventPosition = streamInfo.LastEventNumber;

            if (streamInfo.Exists) return streamInfo.LastEventNumber.Some();

            var message = streamInfo.IsDeleted ? "is a deleted stream" : "does not exist";
            Output.WriteWarning($"Stream {StreamName} {message}");
            return 1L.None();
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
