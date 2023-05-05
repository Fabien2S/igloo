using Igloo.Buffers;
using Igloo.Network.Packets;

namespace Igloo.Network.Login.Outgoing;

public record PacketOutAuthFailed(string Json) : IPacketOut<PacketOutAuthFailed>
{
    public static int Id => 0x00;

    public static void Serialize(ref BufferWriter writer, in PacketOutAuthFailed value)
    {
        writer.WriteString(value.Json);
    }

    public static void Deserialize(ref BufferReader reader, out PacketOutAuthFailed value)
    {
        value = new PacketOutAuthFailed(
            reader.ReadString()
        );
    }
}