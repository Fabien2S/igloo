using System.Buffers;
using System.IO.Pipelines;
using Igloo.Common.Buffers;
using Igloo.Network.Handshake;
using Igloo.Network.Login;
using Microsoft.Extensions.Logging;

namespace Igloo.Network;

public partial class NetworkConnection
{
    private async Task ReceiveAsync()
    {
        var writeTask = FillIncomingAsync(_inPipe.Writer);
        var readTask = ProcessIncomingAsync(_inPipe.Reader);

        await Task.WhenAll(writeTask, readTask).ConfigureAwait(false);
    }

    private async Task FillIncomingAsync(PipeWriter writer)
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var memory = writer.GetMemory(4096);
                var received = await _socket.ReceiveAsync(memory, _cts.Token).ConfigureAwait(false);
                if (received == 0)
                {
                    Close(NetworkReason.ClosedRemotely);
                    break;
                }

                writer.Advance(received);
            }
            catch (Exception e)
            {
                if (HandleException(e, "Failed to receive data"))
                {
                    Close(_cts.IsCancellationRequested ? NetworkReason.ClosedLocally : NetworkReason.SocketError);
                }
                else
                {
                    Close(NetworkReason.ClosedRemotely);
                }

                break;
            }

            var result = await writer.FlushAsync().ConfigureAwait(false);
            if (result.IsCompleted) break;
        }

        await writer.CompleteAsync().ConfigureAwait(false);
        await _outPipe.Writer.CompleteAsync().ConfigureAwait(false);
    }

    private async Task ProcessIncomingAsync(PipeReader reader)
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
                Logger.LogDebug("{}: operation was cancelled", nameof(ProcessIncomingAsync));
                break;
            }

            try
            {
                var buffer = result.Buffer;
                if (HandlePacket(buffer, out var read))
                {
                    var consumed = buffer.GetPosition(read);
                    reader.AdvanceTo(consumed);
                }
                else
                {
                    var examined = buffer.GetPosition(read);
                    reader.AdvanceTo(buffer.Start, examined);
                }
            }
            catch (Exception e)
            {
                HandleException(e, "Protocol error");
                Close(NetworkReason.ProtocolError);
                break;
            }

            if (result.IsCompleted)
            {
                Logger.LogDebug("{}: operation was completed", nameof(ProcessIncomingAsync));
                break;
            }
        }

        await reader.CompleteAsync();
    }

    private bool HandlePacket(ReadOnlySequence<byte> sequence, out int read)
    {
        var headerReader = new SequenceReader<byte>(sequence);
        if (!headerReader.TryReadVarInt32(out var packetLength, out var headerSize))
        {
            // packet length not fully received yet
            read = headerSize;
            return false;
        }

        if (headerSize > NetworkProtocol.PacketHeaderMaxSize)
        {
            // Packet too large
            Close(NetworkReason.ProtocolError);
            read = 0;
            return false;
        }

        if (!headerReader.TryReadExact(packetLength, out var packetSequence))
        {
            // packet not fully received yet
            read = headerSize;
            return false;
        }

        // TODO Replace ToArray() with pooling
        var packetBuffer = packetSequence.ToArray();
        var packetReader = new BufferReader(packetBuffer);

        var packetId = packetReader.ReadVarInt32();
        switch (packetId)
        {
            case 0:
                HandshakePacket.Deserialize(ref packetReader, out var handshakePacket);
                Logger.LogInformation("Received handshake: {}", handshakePacket);

                if (handshakePacket.RequestedState == HandshakePacket.State.Login)
                {
                    _ = WriteAsync(new DisconnectPacket("""
                        {"text":"Test","color":"red"}
                        """));
                }
                else
                {
                    Close(NetworkReason.ClosedLocally);
                }

                break;
            default:
                Logger.LogWarning("Invalid packet {} received from {}", packetId, this);
                Close(NetworkReason.ClosedLocally);
                break;
        }

        read = (int)headerReader.Consumed;
        return true;
    }
}