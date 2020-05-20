using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.Shell.EventStore;

namespace EventStore.Shell.Commands
{
    public class StreamsCommand : Command
    {
        public StreamsCommand() : base("streams", "List all streams (except system streams)")
        {
            var more = new Command("more", "Retrieve the next page of streams")
            {
                Handler = CommandHandler.Create(() => Handle(-1))
            };
            
            Handler = CommandHandler.Create(() => Handle(0));
            AddCommand(more);
        }

        async Task Handle(long start)
        {
            var ctx = SessionContext.Current.Connection;
            if (!await ctx.EnsureConnectedAndLog()) return;

            await ctx.SetStream("$streams");
            var (events, isEndOfStream) = await ctx.CurrentStream.ReadForward(start);
            LogStreams(events, isEndOfStream);
        }

        static void LogStreams(ResolvedEvent[] events, bool isEndOfStream)
        {
            foreach (var evt in events)
            {
                Output.Write(evt.Event.EventStreamId);
            }
            
            if (isEndOfStream)
                Output.WriteWarning("No more streams to fetch");
        }
    }
}
