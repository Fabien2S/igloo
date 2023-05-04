using Igloo.Common.Buffers;

namespace Igloo.Network.Packets;

public delegate void PacketHandler(NetworkConnection connection);

internal static class PacketSerializer
{
    public static bool Deserialize<TPacket>(ref BufferReader reader, out PacketHandler handler) where TPacket : struct, INetworkPacket<TPacket>
    {
        TPacket.Deserialize(ref reader, out var packet);
        handler = connection => TPacket.Handle(connection, packet);
        return true;
    }

    public static bool Empty(out PacketHandler handler)
    {
        handler = static _ => { };
        return false;
    }
}