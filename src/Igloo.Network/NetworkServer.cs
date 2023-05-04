using System.Net;
using System.Net.Sockets;
using Igloo.Logging;
using Igloo.Timings;
using Microsoft.Extensions.Logging;

namespace Igloo.Network;

public class NetworkServer : ITickable, IDisposable
{
    private static readonly ILogger<NetworkServer> Logger = LogManager.Create<NetworkServer>();

    private readonly Socket _listenSocket;
    private readonly CancellationTokenSource _cts;

    private readonly HashSet<NetworkConnection> _connections = new();

    public NetworkServer()
    {
        _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _cts = new CancellationTokenSource();
    }

    public void Tick(in TimeSpan deltaTime)
    {
        lock (_connections)
        {
            foreach (var connection in _connections)
            {
                connection.Tick(in deltaTime);
            }
        }
    }

    public void Listen(IPEndPoint endPoint)
    {
        Logger.LogDebug("Starting network server");

        // Configure socket
        _listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        _listenSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
        _listenSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.TypeOfService, 0x18);

        // Bind and Listen
        _listenSocket.Bind(endPoint);
        _listenSocket.Listen();

        Logger.LogInformation("Listening on {}", endPoint);

        // Accept socket
        _ = AcceptAsync();
    }

    public void Close()
    {
        Logger.LogDebug("Shutting down network server");

        _listenSocket.Shutdown(SocketShutdown.Both);
        _listenSocket.Close();
    }

    private async Task AcceptAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var clientSocket = await _listenSocket.AcceptAsync().ConfigureAwait(false);
                HandleConnection(clientSocket);
            }
            catch (Exception e)
            {
                Logger.LogCritical(e, "Failed to accept connection");
            }
        }
    }

    private void HandleConnection(Socket clientSocket)
    {
        Logger.LogInformation("Connection made from {}", clientSocket.RemoteEndPoint);

        var connection = new NetworkConnection(clientSocket);

        lock (_connections)
        {
            _connections.Add(connection);
        }

        connection.Listen();
    }

    internal void HandleDisconnection(NetworkConnection connection, NetworkReason reason)
    {
        Logger.LogInformation("{} disconnected ({})", connection, reason);
        lock (_connections)
        {
            _connections.Remove(connection);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _listenSocket.Dispose();
    }
}