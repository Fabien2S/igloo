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

    private static async void WaitForClosingAsync(Task readTask, Task writeTask)
    {
        await Task.WhenAll(readTask, writeTask).ConfigureAwait(false);
        // TODO Closing
    }

    public void Listen()
    {
        _readTask = ReceiveAsync();
        _writeTask = SendAsync();
        WaitForClosingAsync(_readTask, _writeTask);
    }

    public async Task WriteAsync<TPacket>(TPacket packet) where TPacket : struct, INetworkPacket<TPacket>
    {
        await _outChannel.Writer.WriteAsync(new WritablePacket<TPacket>(packet), _cts.Token);
    }

    public async ValueTask SendAsync(ReadOnlyMemory<byte> data)
    {
        var writer = _outPipe.Writer;
        await writer.WriteAsync(data).ConfigureAwait(false);
    }

    public override string ToString()
    {
        return $"NetworkConnection[{_socket.RemoteEndPoint}]";
    }
}