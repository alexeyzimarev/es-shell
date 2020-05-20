using System.CommandLine;
using System.CommandLine.Invocation;
using EventStore.Shell.EventStore;

namespace EventStore.Shell.Commands.Stream
{
    public class SetStream : Command
    {
        public SetStream() : base("set", "Set the current stream")
        {
            Handler = CommandHandler.Create<string>(SessionContext.TrySetAndGetCurrentStream);
            AddArgument(new Argument<string>("name"));
        }
    }
}
