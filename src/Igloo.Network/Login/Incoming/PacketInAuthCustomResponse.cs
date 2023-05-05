using Igloo.Buffers;
using Igloo.Network.Packets;

namespace Igloo.Network.Login.Incoming;

public record PacketInAuthCustomResponse(int TransactionId, byte[]? Payload = null) : IPacketIn<PacketInAuthCustomResponse, LoginNetworkHandler>
{
    public static int Id => 0x02;

    public static void Serialize(ref BufferWriter writer, in PacketInAuthCustomResponse value)
    {
        writer.WriteVarInt(value.TransactionId);

        if (value.Payload != null)
        {
            writer.WriteBool(true);
            writer.WriteBytes(value.Payload);
        }
        else
        {
            writer.WriteBool(false);
        }
    }

    public static void Deserialize(ref BufferReader reader, out PacketInAuthCustomResponse value)
    {
        value = new PacketInAuthCustomResponse(
            reader.ReadVarInt(),
            reader.ReadBool() ? reader.RemainingSpan.ToArray() : null
        );
    }
}