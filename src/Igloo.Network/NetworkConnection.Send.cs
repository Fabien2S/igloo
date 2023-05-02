using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Channels;
using Igloo.Common.Buffers;
using Igloo.Network.Packets;
using Microsoft.Extensions.Logging;

namespace Igloo.Network;

public partial class NetworkConnection
{
    private async Task SendAsync()
    {
        var packetEncoder = EncodeOutgoingPacketAsync(_outChannel.Reader, _outPipe.Writer);
        var sendTask = SendOutgoingBytesAsync(_outPipe.Reader);

        await Task.WhenAll(packetEncoder, sendTask).ConfigureAwait(false);
    }

    private async Task EncodeOutgoingPacketAsync(ChannelReader<IWritablePacket> reader, PipeWriter writer)
    {
        while (!_cts.IsCancellationRequested)
        {
            var packet = await reader.ReadAsync(_cts.Token).ConfigureAwait(false);
            Logger.LogDebug("Sending packet {} to {}", packet, this);


            var bodyBuffer = new ArrayBufferWriter<byte>();
            var bodyWriter = new BufferWriter(bodyBuffer);
            packet.Serialize(ref bodyWriter);

            var packetWriter = new BufferWriter(writer);
            packetWriter.WriteVarInt32(bodyBuffer.WrittenCount, out var headerSize);
            packetWriter.WriteBytes(bodyBuffer.WrittenSpan);


            writer.Advance(headerSize + bodyBuffer.WrittenCount);

            // notify the reader
            var result = await writer.FlushAsync().ConfigureAwait(false);
            if (result.IsCompleted) break;
        }
    }

    private async Task SendOutgoingBytesAsync(PipeReader reader)
    {
        while (!_cts.IsCancellationRequested)
        {
            var result = await reader.ReadAsync(_cts.Token).ConfigureAwait(false);

            if (result.IsCanceled)
            {
                Logger.LogDebug("{}: operation was cancelled", nameof(SendAsync));
                break;
            }

            try
            {
                var buffer = result.Buffer;
                var sent = await SendBufferAsync(buffer).ConfigureAwait(false);
                Logger.LogDebug("Sending {} byte(s) to {}", sent, this);

                var consumed = buffer.GetPosition(sent);
                reader.AdvanceTo(consumed);
            }
            catch (Exception e)
            {
                Logger.LogCritical(e, "{}: failed to send data", nameof(SendAsync));
                _cts.Cancel(false);
                break;
            }

            if (result.IsCompleted)
            {
                Logger.LogDebug("{}: operation was completed", nameof(SendAsync));
                break;
            }
        }

        await reader.CompleteAsync().ConfigureAwait(false);
    }

    private async Task<int> SendBufferAsync(ReadOnlySequence<byte> buffer)
    {
        if (buffer.IsSingleSegment)
        {
            return await _socket.SendAsync(buffer.First, SocketFlags.None, _cts.Token).ConfigureAwait(false);
        }

        var sent = 0;
        foreach (var memory in buffer)
        {
            sent += await _socket.SendAsync(memory, SocketFlags.None, _cts.Token).ConfigureAwait(false);
        }

        return sent;
    }
}