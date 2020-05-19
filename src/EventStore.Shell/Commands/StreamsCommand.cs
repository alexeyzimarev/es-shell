using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace EventStore.Shell.Commands
{
    public class StreamsCommand : Command
    {
        const string Streams = "$streams";
        
        public StreamsCommand() : base("streams", "List all streams (except system streams)")
        {
            Handler = CommandHandler.Create(Handle);
        }

        async Task Handle()
        {
            var connection = ConnectionManager.GetCurrentConnection();
            if (connection == null) return;
            
            var slice = await connection.ReadStreamEventsForwardAsync(Streams, 0, 10, true);

            foreach (var evt in slice.Events)
            {
                Output.Write(evt.Event.EventStreamId);
            }
        }
    }
}
