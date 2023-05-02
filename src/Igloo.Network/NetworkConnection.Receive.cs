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
            var memory = writer.GetMemory();
            var received = await _socket.ReceiveAsync(memory, _cts.Token).ConfigureAwait(false);
            if (received == 0)
            {
                // disconnected
                break;
            }

            writer.Advance(received);

            // notify the reader
            var result = await writer.FlushAsync().ConfigureAwait(false);
            if (result.IsCompleted) break;
        }

        await writer.CompleteAsync().ConfigureAwait(false);
    }

    private async Task ProcessIncomingAsync(PipeReader reader)
    {
        while (!_cts.IsCancellationRequested)
        {
            var result = await reader.ReadAsync(_cts.Token).ConfigureAwait(false);
            if (result.IsCanceled)
            {
                Logger.LogDebug("{}: operation was cancelled", nameof(ProcessIncomingAsync));
                break;
            }

            var buffer = result.Buffer;

            try
            {
                HandlePacket(buffer);

                if (result.IsCompleted)
                {
                    Logger.LogDebug("{}: operation was completed", nameof(ProcessIncomingAsync));
                    break;
                }
            }
            finally
            {
                reader.AdvanceTo(buffer.Start, buffer.End);
            }
        }

        await reader.CompleteAsync();
    }

    private void HandlePacket(ReadOnlySequence<byte> sequence)
    {
        var arrayPool = ArrayPool<byte>.Shared;

        Logger.LogDebug("Received sequence {}", sequence.ToString());
        var buffer = arrayPool.Rent((int)sequence.Length);
        try
        {
            sequence.CopyTo(buffer);

            var reader = new BufferReader(buffer);

            var packetLength = reader.ReadVarInt32(NetworkProtocol.PacketHeaderMaxSize);
            var packetBuffer = reader.ReadBytes(packetLength);

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

                    break;
                default:
                    Logger.LogWarning("Invalid packet: {}", packetId);
                    break;
            }
        }
        finally
        {
            arrayPool.Return(buffer);
        }
    }
}