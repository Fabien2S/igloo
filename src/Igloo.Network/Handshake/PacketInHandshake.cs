using Igloo.Buffers;
using Igloo.Network.Packets;

namespace Igloo.Network.Handshake;

public record PacketInHandshake
    (int Protocol, string Address, ushort Port, PacketInHandshake.State RequestedState) : IPacketIn<PacketInHandshake, HandshakeNetworkHandler>
{
    public static int Id => 0x00;

    public enum State
    {
        Handshake,
        Status,
        Login
    }

    public static void Serialize(ref BufferWriter writer, in PacketInHandshake value)
    {
        writer.WriteVarInt(value.Protocol);
        writer.WriteString(value.Address, byte.MaxValue);
        writer.WriteUShort(value.Port);
        writer.WriteVarInt((int)value.RequestedState);
    }

    public static void Deserialize(ref BufferReader reader, out PacketInHandshake value)
    {
        value = new PacketInHandshake(
            reader.ReadVarInt(),
            reader.ReadString(byte.MaxValue),
            reader.ReadUShort(),
            (State)reader.ReadVarInt()
        );
    }
}