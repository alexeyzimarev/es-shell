using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace EventStore.Shell
{
    public class ConnectionManager
    {
        string _host;

        public static string ConnectedTo => Instance._host ?? "(disconnected)";
        
        public static ConnectionManager Instance { get; } = new ConnectionManager();

        public static IEventStoreConnection GetCurrentConnection()
        {
            var connection = Instance.Connection;

            if (connection == null)
            {
                Output.WriteError("Not connected to Event Store, use the connect command");
                return null;
            }

            return connection;
        }
        
        IEventStoreConnection Connection { get; set; }

        public async Task Connect(string host, string user, string password)
        {
            var connectionString = $"ConnectTo=tcp://{user}:{password}@{host}:1113; HeartBeatTimeout=500";

            Connection = EventStoreConnection.Create(connectionString);
            
            Output.WriteInfo($"Connecting to {host}...");
            await Connection.ConnectAsync();
            Output.WriteInfo($"Connected to {host}");

            _host = host;
        }

        public void Disconnect()
        {
            if (Connection == null) return;
            
            Output.WriteInfo("Closing the connection...");
            Connection.Close();
            Output.WriteInfo("Connection closed");
        }
    }
}
