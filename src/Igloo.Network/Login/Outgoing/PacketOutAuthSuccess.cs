using Igloo.Buffers;
using Igloo.Network.Packets;
using Igloo.Profiles;

namespace Igloo.Network.Login.Outgoing;

public record PacketOutAuthSuccess(GameProfile Profile) : IPacketOut<PacketOutAuthSuccess>
{
    public static int Id => 0x02;

    public static void Serialize(ref BufferWriter writer, in PacketOutAuthSuccess value)
    {
        var (uuid, name, properties) = value.Profile;
        writer.WriteUuid(uuid);
        writer.WriteString(name, GameProfile.UsernameMaxLength);
        writer.WriteVarInt(properties.Length);
        foreach (var (propName, propValue, propSignature) in properties)
        {
            writer.WriteString(propName);
            writer.WriteString(propValue);
            if (string.IsNullOrEmpty(propSignature))
            {
                writer.WriteBool(false);
            }
            else
            {
                writer.WriteBool(true);
                writer.WriteString(propSignature);
            }
        }
    }
}