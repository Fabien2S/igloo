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
    private readonly ArrayBufferWriter<byte> _outgoingBuffer = new();

    private async Task SendAsync()
    {
        var packetEncoder = EncodeOutgoingPacketAsync(_outChannel.Reader, _outPipe.Writer);
        var sendTask = SendOutgoingBytesAsync(_outPipe.Reader);

        await Task.WhenAll(packetEncoder, sendTask).ConfigureAwait(false);
    }

    private async Task EncodeOutgoingPacketAsync(ChannelReader<PacketSerializer> reader, PipeWriter writer)
    {
        static void WritePacket(IBufferWriter<byte> buffer, PacketSerializer serializer)
        {
            var writer = new BufferWriter(buffer);
            serializer(ref writer);
        }

        static int PrependVarIntLength(PipeWriter writer, ReadOnlySpan<byte> packet)
        {
            var bufferWriter = new BufferWriter(writer);
            bufferWriter.WriteVarInt32(packet.Length, out var packetHeader);
            bufferWriter.WriteBytes(packet);
            return packetHeader + packet.Length;
        }

        while (!_cts.IsCancellationRequested)
        {
            var packetSerializer = await reader.ReadAsync(_cts.Token).ConfigureAwait(false);
            Logger.LogTrace("Sending packet {} to {}", packetSerializer, this);

            _outgoingBuffer.Clear();
            WritePacket(_outgoingBuffer, packetSerializer);

            var written = PrependVarIntLength(writer, _outgoingBuffer.WrittenSpan);
            writer.Advance(written);

            var result = await writer.FlushAsync().ConfigureAwait(false);
            if (result.IsCompleted) break;
        }
    }

    private async Task SendOutgoingBytesAsync(PipeReader reader)
    {
        while (!_cts.IsCancellationRequested)
        {
            ReadResult result;
            try
            {
                result = await reader.ReadAsync(_cts.Token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                HandleException(e, "Failed to read from pipe");
                break;
            }

            if (result.IsCanceled)
            {
                Logger.LogDebug("{}: operation was cancelled", nameof(SendOutgoingBytesAsync));
                break;
            }

            var buffer = result.Buffer;

            try
            {
                Logger.LogTrace("Sending {} byte(s) to {}", buffer.Length, this);
                var sent = await SendBufferAsync(buffer).ConfigureAwait(false);

                var consumed = buffer.GetPosition(sent);
                reader.AdvanceTo(consumed);
            }
            catch (Exception e)
            {
                _cts.Cancel(false);
                HandleException(e, "Failed to send data");
                break;
            }

            if (result.IsCompleted)
            {
                Logger.LogDebug("{}: operation was completed", nameof(SendOutgoingBytesAsync));
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