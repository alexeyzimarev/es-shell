using System;
using System.CommandLine;
using System.Threading.Tasks;
using EventStore.Shell.Commands;

namespace EventStore.Shell
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var command = new Root();

            var result = 0;
            ReadLine.ReadLine.AutoCompletionHandler = new AutoCompletionHandler(command);
            ReadLine.ReadLine.HistoryEnabled = true;
            
            while (result != 255)
            {
                var input = ReadLine.ReadLine.Read("> ");
                if (input.StartsWith("help")) input = "--" + input;
                result = await command.InvokeAsync(input);
            }
            
            ConnectionManager.Instance.Disconnect();
            Output.WriteInfo("Bye :)");
        }
    }
}
