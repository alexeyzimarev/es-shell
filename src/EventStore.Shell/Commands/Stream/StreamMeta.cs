using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using EventStore.Shell.EventStore;

namespace EventStore.Shell.Commands.Stream
{
    public class StreamMeta : Command
    {
        public StreamMeta() : base("meta", "Read stream metadata")
        {
            Handler = CommandHandler.Create<string>(HandleReadMeta);
            AddArgument(new Argument<string>("name", () => null));
        }

        static async Task HandleReadMeta(string name)
        {
            var stream = await SessionContext.TrySetAndGetCurrentStream(name);
            if (stream == null) return;

            var meta = await stream.ReadMeta();

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
    }
}
