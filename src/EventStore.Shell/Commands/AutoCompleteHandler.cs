using System.CommandLine;
using System.Linq;
using EventStore.Shell.ReadLine;

namespace EventStore.Shell.Commands
{
    class AutoCompletionHandler : IAutoCompleteHandler
    {
        readonly RootCommand _rootCommand;
        
        public AutoCompletionHandler(RootCommand rootCommand) => _rootCommand = rootCommand;

        public char[] Separators { get; set; } = { ' ', '.', '/', '\\', ':' };
        
        public string[] GetSuggestions(string text, int index)
        {
            if (index == 0)
            {
                return _rootCommand.Children.Select(x => x.Name).Where(x => x.StartsWith(text)).ToArray();
            }

            return null;
        }
    }
}
