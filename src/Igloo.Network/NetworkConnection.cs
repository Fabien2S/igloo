using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Channels;
using Igloo.Common.Buffers;
using Igloo.Common.Logging;
using Igloo.Common.Timings;
using Igloo.Network.Handlers;
using Igloo.Network.Handshake;
using Igloo.Network.Packets;
using Microsoft.Extensions.Logging;

namespace Igloo.Network;

public partial class NetworkConnection : ITickable
{
    private static readonly ILogger<NetworkConnection> Logger = LogManager.Create<NetworkConnection>();

    public INetworkHandler Handler
    {
        get => _handler;
        set => _handler = value ?? throw new ArgumentNullException(nameof(value));
    }

    private readonly Socket _socket;
    private readonly CancellationTokenSource _cts;

    private readonly Pipe _inPipe;
    private readonly Pipe _outPipe;

    private readonly Channel<PacketHandler> _inChannel;
    private readonly Channel<PacketSerializer> _outChannel;

    private Task? _readTask;
    private Task? _writeTask;

    private INetworkHandler _handler;
    private NetworkReason _reason;

    public NetworkConnection(Socket socket)
    {
        _socket = socket;
        _cts = new CancellationTokenSource();

        _inPipe = new Pipe();
        _outPipe = new Pipe();

        _inChannel = Channel.CreateUnbounded<PacketHandler>(new UnboundedChannelOptions
        {
        });
        _outChannel = Channel.CreateUnbounded<PacketSerializer>(new UnboundedChannelOptions
        {
        });

        _handler = new HandshakeNetworkHandler();
    }

    public void Tick(in TimeSpan deltaTime)
    {
        var reader = _inChannel.Reader;
        while (reader.TryRead(out var handler))
            handler(this);
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
        _readTask = ReceiveAsync();
        _writeTask = SendAsync();
        WaitForClosingAsync(_readTask, _writeTask);
    }

    public async Task SendAsync<TPacket>(TPacket packet) where TPacket : struct, INetworkPacket<TPacket>
    {
        await _outChannel.Writer.WriteAsync((ref BufferWriter writer) =>
        {
            writer.WriteVarInt32(TPacket.Id);
            TPacket.Serialize(ref writer, in packet);
        }, _cts.Token);
    }

    public void Close(NetworkReason reason)
    {
        if (_reason != NetworkReason.Ok)
            return;

        _reason = reason;
        _cts.Cancel();
    }

    public override string ToString()
    {
        return $"NetworkConnection[{_socket.RemoteEndPoint}]";
    }
}