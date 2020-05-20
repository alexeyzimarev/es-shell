using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace EventStore.Shell.Commands
{
    public class Root : RootCommand
    {
        public Root()
        {
            Name = "es-shell";
            Handler = CommandHandler.Create(() => Console.Out.WriteLineAsync("Root called"));
            
            AddCommand(new Command("exit") {Handler = CommandHandler.Create(() => Task.FromResult(255))});
            AddCommand(new ConnectCommand());
            AddCommand(new StreamsCommand());
            AddCommand(new StreamCommand());
        }

        void AddSuggestion(Command command)
        {
        }
    }
}
