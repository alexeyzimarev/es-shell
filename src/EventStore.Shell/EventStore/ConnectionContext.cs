using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace EventStore.Shell.EventStore
{
    public class ConnectionContext : IDisposable
    {
        readonly SessionContext _sessionContext;
        public   StreamContext  CurrentStream { get; private set; }

        public string ConnectedTo  => $"[{_host}] {StreamStatus}";
        string        StreamStatus => CurrentStream == null ? "" : $"[{CurrentStream.Status}]";

        string _host;

        public ConnectionContext(SessionContext sessionContext) => _sessionContext = sessionContext;

        public async Task<IEventStoreConnection> GetCurrentConnection()
        {
            if (_connection == null)
            {
                Output.WriteError("Not connected to Event Store, use the connect command");
                return null;
            }

            return await EnsureConnectedAndLog() ? _connection : null;
        }

        public async Task<bool> EnsureConnectedAndLog()
        {
            var (ok, error) = await EnsureConnected();
            if (!ok) Output.WriteError($"Not connected to Event Store. {error}");
            return ok;
        }

        IEventStoreConnection _connection;

        public async Task<bool> Connect(string host, string user, string password)
        {
            var connectionString = $"ConnectTo=tcp://{user}:{password}@{host}:1113; HeartBeatTimeout=500";

            var settingsBuilder = ConnectionSettings
                .Create()
                .KeepReconnecting()
                .LimitReconnectionsTo(10);

            _connection = EventStoreConnection.Create(connectionString, settingsBuilder);

            Output.WriteInfo($"Connecting to {host}...");
            await _connection.ConnectAsync();

            var (ok, error) = await EnsureConnected();

            if (!ok)
            {
                Output.WriteError($"Unable to connect to {host}. {error}");
                return false;
            }

            _connection.Disconnected += (sender, args) =>
            {
                Output.WriteWarning("Connection closed");
                _sessionContext.Disconnect();
            };
            _connection.Connected    += (sender, args) => Output.WriteInfo("Connected");
            _connection.Reconnecting += (sender, args) => Output.WriteWarning("Reconnecting...");

            _host = host;
            return true;
        }

        public void Disconnect()
        {
            if (_connection == null) return;

            Output.WriteInfo("Closing the connection...");
            _connection.Close();
            Output.WriteInfo("Connection closed");
        }

        public async Task SetStream(string name)
        {
            if (CurrentStream?.StreamName == name) return;

            var stream     = new StreamContext(this, name);
            var connection = await GetCurrentConnection();
            var check      = await stream.GetLastEventNumber(connection);

            check.MatchSome(
                x =>
                {
                    Output.WriteInfo($"Selected stream {name}, last event number is {x}");
                    CurrentStream = stream;
                }
            );
        }

        public void Dispose()
        {
            Disconnect();
            _connection?.Dispose();
            _connection = null;
        }

        async Task<(bool Ok, string Error)> EnsureConnected()
        {
            try
            {
                await _connection.GetStreams(0, 1);
                return (true, "");
            }
            catch (ObjectDisposedException)
            {
                return (false, "Cannot reach Event Store or the connection was closed");
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
        }
    }
}
