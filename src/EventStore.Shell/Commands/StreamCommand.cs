using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using EventStore.Shell.EventStore;

namespace EventStore.Shell.Commands
{
    public class StreamCommand : Command
    {
        public StreamCommand() : base("stream", "Work with a single stream")
        {
            var setStream = new Command("set", "Set the current stream") {Handler = CommandHandler.Create<string>(HandleSetStream)};
            setStream.AddArgument(new Argument<string>("name"));

            var readStream = new Command("read", "Read events from the stream")
                {Handler = CommandHandler.Create<string, long, int>(HandleReadStream)};
            readStream.AddArgument(new Argument<string>("name", () => null));
            readStream.AddOption(new Option<long>("--start", () => -1, "Start position"));
            readStream.AddOption(new Option<int>("--count", () => 10, "Number of events to read"));

            var meta = new Command("meta", "Read stream metadata") {Handler = CommandHandler.Create<string>(HandleReadMeta)};
            meta.AddArgument(new Argument<string>("name", () => null));

            AddCommand(setStream);
            AddCommand(readStream);
            AddCommand(meta);
        }

        static async Task HandleReadMeta(string name)
        {
            if (name != null) await Stream.SetCurrent(name);

            var meta = await Stream.Current.ReadMeta();

            meta.MatchSome(
                x =>
                {
                    Output.WriteValue("Meta stream version", x.MetastreamVersion);
                    Output.WriteValue("Max age", x.StreamMetadata.MaxAge);
                    Output.WriteValue("Max count", x.StreamMetadata.MaxCount);
                    Output.WriteValue("Truncate before", x.StreamMetadata.TruncateBefore);

                    foreach (var (key, value) in x.StreamMetadata.CustomMetadataAsRawJsons)
                    {
                        Output.WriteValue($"[Custom] {key}", value);
                    }
                }
            );
        }

        static async Task HandleReadStream(string name, long start, int count)
        {
            if (name != null) await Stream.SetCurrent(name);

            var events = await Stream.Current.ReadForward(start, count);

            if (events.Length == 0)
            {
                Output.WriteWarning($"No more events found in the stream {name}");
                return;
            }

            foreach (var evt in events)
            {
                Output.WriteValue("Type     ", evt.Event.EventType);
                Output.WriteValue("Id       ", evt.Event.EventId);
                Output.WriteValue("Sequence ", evt.Event.EventNumber);
                Output.WriteValue("Timestamp", evt.Event.Created);
                Output.Write("---------");
            }

            string Deserialize(byte[] data)
            {
                return "";
            }
        }

        static Task HandleSetStream(string name) => Stream.SetCurrent(name);
    }
}
