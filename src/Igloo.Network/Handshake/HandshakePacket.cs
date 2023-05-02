using Igloo.Common.Buffers;
using Igloo.Network.Packets;

namespace Igloo.Network.Handshake;

public readonly record struct HandshakePacket(int Protocol, string Address, ushort Port, HandshakePacket.State RequestedState) : INetworkPacket<HandshakePacket>
{
    public static int Id => 0x00;

    public enum State
    {
        Handshake,
        Status,
        Login
    }

    public static void Serialize(ref BufferWriter writer, in HandshakePacket value)
    {
        writer.WriteVarInt32(value.Protocol);
        writer.WriteString(value.Address, byte.MaxValue);
        writer.WriteUShort(value.Port);
        writer.WriteVarInt32((int)value.RequestedState);
    }

    public static void Deserialize(ref BufferReader reader, out HandshakePacket value)
    {
        value = new HandshakePacket(
            reader.ReadVarInt32(),
            reader.ReadString(byte.MaxValue),
            reader.ReadUShort(),
            (State)reader.ReadVarInt32()
        );
    }
}