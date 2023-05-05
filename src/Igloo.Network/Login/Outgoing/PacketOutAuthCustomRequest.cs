using Igloo.Buffers;
using Igloo.Network.Packets;

namespace Igloo.Network.Login.Outgoing;

public record PacketOutAuthCustomRequest(int TransactionId, Identifier Channel, byte[] Payload) : IPacketOut<PacketOutAuthCustomRequest>
{
    public const int PayloadMaxSize = 0x100000;

    public static int Id => 0x04;

    public static void Serialize(ref BufferWriter writer, in PacketOutAuthCustomRequest value)
    {
        writer.WriteVarInt(value.TransactionId);
        writer.WriteIdentifier(value.Channel);
        writer.WriteBytes(value.Payload);
    }

    public static void Deserialize(ref BufferReader reader, out PacketOutAuthCustomRequest value)
    {
        value = new PacketOutAuthCustomRequest(
            reader.ReadVarInt(),
            reader.ReadIdentifier(),
            reader.RemainingSpan.ToArray()
        );

        var payload = value.Payload;
        if (payload.Length > PayloadMaxSize)
        {
            throw new IOException($"Payload is too large ({payload.Length} > {PayloadMaxSize})");
        }
    }
}