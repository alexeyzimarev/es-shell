using System;
using System.Threading.Tasks;

namespace EventStore.Shell.EventStore
{
    public class SessionContext : IDisposable
    {
        public static SessionContext Current { get; } = new SessionContext();
        
        public ConnectionContext Connection { get; private set; }

        public bool IsConnected => Connection != null;

        public async Task Connect(string host, string user, string password)
        {
            Connection?.Dispose();
            
            Connection = new ConnectionContext(this);
            if (!await Connection.Connect(host, user, password)) Disconnect();
        }

        public void Disconnect()
        {
            Connection?.Dispose();
            Connection = null;
        }

        public string Status => Connection == null 
            ? "[disconnected]" 
            : Connection?.ConnectedTo;

        public void Dispose() => Disconnect();

        public static bool TryGetConnectionContext(out ConnectionContext context)
        {
            if (Current.Connection == null)
                Output.WriteError("Not connected to Event Store, use the 'connect' command");
            
            context = Current.Connection;
            return context != null;
        }

        public static bool TryGetCurrentStream(out StreamContext context)
        {
            if (Current.Connection.CurrentStream == null)
                Output.WriteError("Stream context not set, either use the '--name' option or the 'stream set' command");
            
            context = Current.Connection.CurrentStream;
            return context != null;
        }

        public static async Task<StreamContext> TrySetAndGetCurrentStream(string streamName)
        {
            if (!TryGetConnectionContext(out var connection)) return null;
            
            if (streamName != null) await connection.SetStream(streamName);

            return TryGetCurrentStream(out var stream) ? stream : null;
        }
    }
}
