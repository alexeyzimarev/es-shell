using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.Shell.EventStore;

namespace EventStore.Shell.Commands.Stream
{
    public class ReadStream : Command
    {
        public ReadStream() : base("read", "Read events from the stream")
        {
            Handler = CommandHandler.Create<string, long, int, bool, bool>(HandleReadStream);
            AddArgument(new Argument<string>("name", () => null));
            AddOption(new Option<long>("--start", () => -1, "Start position"));
            AddOption(new Option<int>("--count", () => 10, "Number of events to read"));
            AddOption(new Option<bool>("--data", "Show JSON data payload"));
            AddOption(new Option<bool>("--meta", "Show JSON metadata"));
        }

        static async Task HandleReadStream(string name, long start, int count, bool data, bool meta)
        {
            var stream = await SessionContext.TrySetAndGetCurrentStream(name);
            if (stream == null) return;
            
            var (events, isEndOfStream) = await stream.ReadForward(start, count, x => Output.WriteInfo(x));

            if (events.Length == 0)
            {
                Output.WriteWarning($"No more events found in the stream {name}");
                return;
            }

            LogEvents(events, data, meta);

            if (isEndOfStream) Output.WriteWarning("End of the stream");
        }

        static void LogEvents(IEnumerable<ResolvedEvent> events, bool showData, bool showMeta)
        {
            Separate();

            foreach (var evt in events)
            {
                LogEvent(evt);
            }

            void LogEvent(ResolvedEvent evt)
            {
                Output.WriteValue("Sequence ", evt.Event.EventNumber);
                Output.WriteValue("Id       ", evt.Event.EventId);
                Output.WriteValue("Type     ", evt.Event.EventType);
                Output.WriteValue("Timestamp", evt.Event.Created);
                if (showData)
                    Output.Write("Data:", evt.DataAsString());
                if (showMeta)
                    Output.Write("Metadata:", evt.MetaAsString());
                Separate();
            }

            static void Separate() => Output.Write("---------");
        }
    }
}
