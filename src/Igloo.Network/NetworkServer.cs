using System.Net;
using System.Net.Sockets;
using Igloo.Common.Logging;
using Microsoft.Extensions.Logging;

namespace Igloo.Network;

public class NetworkServer : IDisposable
{
    private static readonly ILogger<NetworkServer> Logger = LogManager.Create<NetworkServer>();

    private readonly Socket _listenSocket;
    private readonly SocketAsyncEventArgs _eventArgs;
    private readonly CancellationTokenSource _cts;

    public NetworkServer()
    {
        _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        _eventArgs = new SocketAsyncEventArgs();
        _cts = new CancellationTokenSource();
    }

    public void Listen(IPEndPoint endPoint)
    {
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
        connection.Listen();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _eventArgs.Dispose();
        _listenSocket.Dispose();
    }
}