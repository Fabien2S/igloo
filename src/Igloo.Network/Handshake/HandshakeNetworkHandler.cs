using Igloo.Buffers;
using Igloo.Network.Handlers;
using Igloo.Network.Packets;

namespace Igloo.Network.Handshake;

public class HandshakeNetworkHandler : INetworkHandler
{
    public bool IsAsync => true;

    public bool ReceivePacket(int id, ref BufferReader reader, out PacketHandler handler)
    {
        return id switch
        {
            0 => PacketSerializer.Deserialize<HandshakePacket>(ref reader, out handler),
            _ => PacketSerializer.Empty(out handler)
        };
    }
}