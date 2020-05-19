using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace EventStore.Shell.Commands
{
    public class ConnectCommand : Command
    {
        public ConnectCommand() : base("connect", "Connect to Event Store server")
        {
            AddOption(new Option<string>(new []{"-h", "--host"}, () => "localhost", "Host name or ip address"));
            AddOption(new Option<string>(new []{"-u", "--user"}, () => "admin", "User name"));
            AddOption(new Option<string>(new []{"-p", "--password"}, () => "changeit", "Password"));
            Handler = CommandHandler.Create<string, string, string>(Handle);
        }

        static async Task<int> Handle(string host, string user, string password)
        {
            await ConnectionManager.Instance.Connect(host, user, password);
            return 0;
        }
    }
}
