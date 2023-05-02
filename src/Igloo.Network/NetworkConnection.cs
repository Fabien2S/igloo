using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Channels;
using Igloo.Common.Logging;
using Igloo.Network.Packets;
using Microsoft.Extensions.Logging;

namespace Igloo.Network;

public partial class NetworkConnection
{
    private static readonly ILogger<NetworkConnection> Logger = LogManager.Create<NetworkConnection>();

    private readonly Socket _socket;
    private readonly CancellationTokenSource _cts;

    private readonly Pipe _inPipe;
    private readonly Pipe _outPipe;

    private readonly Channel<IWritablePacket> _inChannel;
    private readonly Channel<IWritablePacket> _outChannel;

    private Task? _readTask;
    private Task? _writeTask;

    private NetworkReason _reason;

    public NetworkConnection(Socket socket)
    {
        _socket = socket;
        _cts = new CancellationTokenSource();

        _inPipe = new Pipe();
        _outPipe = new Pipe();

        _inChannel = Channel.CreateUnbounded<IWritablePacket>(new UnboundedChannelOptions
        {
        });
        _outChannel = Channel.CreateUnbounded<IWritablePacket>(new UnboundedChannelOptions
        {
        });
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
                Logger.LogCritical(exception, details, Array.Empty<object>());
                return true;
        }
    }

    internal void Listen()
    {
        _readTask = ReceiveAsync();
        _writeTask = SendAsync();
        WaitForClosingAsync(_readTask, _writeTask);
    }

    public async Task WriteAsync<TPacket>(TPacket packet) where TPacket : struct, INetworkPacket<TPacket>
    {
        await _outChannel.Writer.WriteAsync(new WritablePacket<TPacket>(packet), _cts.Token);
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