using System.Diagnostics;
using System.Net.Sockets;
using Igloo.Logging;
using Igloo.Network.Handlers;
using Igloo.Network.Handshake;
using Igloo.Network.Packets;
using Igloo.Timings;
using Microsoft.Extensions.Logging;

namespace Igloo.Network;

public partial class NetworkConnection : ITickable
{
    private static readonly ILogger<NetworkConnection> Logger = LogManager.Create<NetworkConnection>();

    // TODO Rework
    public INetworkHandler Handler
    {
        get => _handler;
        set => _handler = value ?? throw new ArgumentNullException(nameof(value));
    }

    private readonly Socket _socket;
    private readonly CancellationTokenSource _cts;

    private Task? _readTask;
    private Task? _writeTask;

    private INetworkHandler _handler;
    private NetworkReason _reason;

    public NetworkConnection(Socket socket)
    {
        _socket = socket;
        _cts = new CancellationTokenSource();

        _handler = new HandshakeNetworkHandler(this);
    }

    public void Tick(in TimeSpan deltaTime)
    {
        var reader = _incomingPackets.Reader;
        while (reader.TryRead(out var packetInvoker))
            packetInvoker.Invoke();
    }

    private async void WaitForClosingAsync(Task readTask, Task writeTask)
    {
        try
        {
            await Task.WhenAll(readTask, writeTask).ConfigureAwait(false);
            Logger.LogInformation("Closing connection {} (reason: {})", this, _reason);
        }
        catch (Exception e)
        {
            HandleException(e, "Waiting for closing");
        }
        finally
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                HandleException(e, "Shutting down socket");
            }
            finally
            {
                _socket.Close();
            }
        }
    }

    private bool HandleException(Exception exception, string details)
    {
        switch (exception)
        {
            case ObjectDisposedException:
            case OperationCanceledException:
            case SocketException
            {
                SocketErrorCode: SocketError.OperationAborted or SocketError.ConnectionReset or SocketError.TimedOut or SocketError.NetworkReset
            }:
                return false;
            default:
                Logger.LogCritical(exception, details);
                return true;
        }
    }

    internal void Listen()
    {
        _readTask = PerformReceiveAsync();
        _writeTask = PerformSendAsync();
        WaitForClosingAsync(_readTask, _writeTask);
    }

    public void Send<TPacket>(in TPacket packet) where TPacket : class, IPacketOut<TPacket>
    {
        Logger.LogTrace("Sending packet {} to {}", packet, this);
        if (!_outgoingPackets.Writer.TryWrite(packet))
            throw new UnreachableException($"{nameof(_outgoingPackets)} should be unbounded");
    }

    public void Close(NetworkReason reason)
    {
        if (_reason != NetworkReason.Ok)
            return;

        Logger.LogInformation("Closing connection with {} ({})", this, reason);
        _reason = reason;
        _cts.Cancel();
    }

    public override string ToString()
    {
        return $"NetworkConnection[{_socket.RemoteEndPoint}]";
    }
}