using Igloo.Common.Buffers;
using Igloo.Common.Logging;
using Igloo.Network.Login;
using Igloo.Network.Packets;
using Microsoft.Extensions.Logging;

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

    public static bool Handle(NetworkConnection connection, in HandshakePacket packet)
    {
        LogManager.Create<HandshakePacket>().LogDebug("Handling packet {} on {}", packet, Thread.CurrentThread);
        if (packet.RequestedState == State.Login)
        {
            connection.Handler = new LoginNetworkHandler();
            _ = connection.SendAsync(new DisconnectPacket(
                """
                {"text":"Test","color":"gold"}
                """
            ));
            return true;
        }

        connection.Close(NetworkReason.ClosedLocally);
        return false;
    }
}