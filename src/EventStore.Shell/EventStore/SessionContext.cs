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
    }
}
