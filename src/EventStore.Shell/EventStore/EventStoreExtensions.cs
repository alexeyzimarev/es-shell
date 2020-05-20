using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace EventStore.Shell.EventStore
{
    public static class EventStoreExtensions
    {
        const string Streams = "$streams";
        
        public static async Task<List<string>> GetStreams(this IEventStoreConnection connection, long start, int count)
        {
            if (connection == null)
            {
                Output.WriteWarning("Not connected, use the connect command");
                return new List<string>();
            }
            
            var slice = await connection.ReadStreamEventsForwardAsync(Streams, start, count, true);

            return slice.Events?.Select(x => x.Event.EventStreamId).ToList() ?? new List<string>();
        }

        public static async Task<long> GetStreamSize(this IEventStoreConnection connection, string streamName)
        {
            var slice = await connection.ReadStreamEventsBackwardAsync(streamName, StreamPosition.End, 1, true);
            return slice.Status == SliceReadStatus.Success ? slice.LastEventNumber : 0;
        }
    }
}
