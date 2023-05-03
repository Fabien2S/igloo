using Igloo.Common.Buffers;
using Igloo.Network.Packets;

namespace Igloo.Network.Login;

public record struct DisconnectPacket(string Json) : INetworkPacket<DisconnectPacket>
{
    public static int Id => 0x00;

    public static void Serialize(ref BufferWriter writer, in DisconnectPacket value)
    {
        writer.WriteString(value.Json);
    }

    public static void Deserialize(ref BufferReader reader, out DisconnectPacket value)
    {
        value = new DisconnectPacket(
            reader.ReadString()
        );
    }

    public static bool Handle(NetworkConnection connection, in DisconnectPacket packet)
    {
        return false;
    }
}