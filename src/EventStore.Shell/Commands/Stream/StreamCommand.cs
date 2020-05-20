using System.CommandLine;

namespace EventStore.Shell.Commands.Stream
{
    public class StreamCommand : Command
    {
        public StreamCommand() : base("stream", "Work with a single stream")
        {
            AddCommand(new SetStream());
            AddCommand(new ReadStream());
            AddCommand(new StreamMeta());
        }
    }
}
