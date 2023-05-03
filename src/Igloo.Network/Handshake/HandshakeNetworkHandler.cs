using Igloo.Common.Buffers;
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
            0 => PacketHelper.Deserialize<HandshakePacket>(ref reader, out handler),
            _ => PacketHelper.Empty(out handler)
        };
    }
}