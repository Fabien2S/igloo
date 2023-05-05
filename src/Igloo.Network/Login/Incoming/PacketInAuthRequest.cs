using Igloo.Buffers;
using Igloo.Extensions;
using Igloo.Network.Packets;

namespace Igloo.Network.Login.Incoming;

public record PacketInAuthRequest(string Username, Guid? Uuid) : IPacketIn<PacketInAuthRequest, LoginNetworkHandler>
{
    public static int Id => 0x00;

    public static void Serialize(ref BufferWriter writer, in PacketInAuthRequest value)
    {
        writer.WriteString(value.Username);
        if (value.Uuid.TryGetValue(out var uuid))
        {
            writer.WriteBool(true);
            writer.WriteUuid(uuid);
        }
        else
        {
            writer.WriteBool(false);
        }
    }

    public static void Deserialize(ref BufferReader reader, out PacketInAuthRequest value)
    {
        value = new PacketInAuthRequest(
            reader.ReadString(),
            reader.ReadBool() ? reader.ReadUuid() : null
        );
    }
}