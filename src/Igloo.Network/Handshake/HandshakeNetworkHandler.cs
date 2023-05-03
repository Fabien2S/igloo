using Igloo.Common.Buffers;
using Igloo.Network.Handlers;
using Igloo.Network.Packets;

namespace Igloo.Network.Handshake;

public class HandshakeNetworkHandler : INetworkHandler
{
    public bool IsAsync => true;

    public bool ReceivePacket(int id, ref BufferReader reader, out PacketHandler handler)
    {
        switch (id)
        {
            case 0:
            {
                HandshakePacket.Deserialize(ref reader, out var handshakePacket);
                handler = connection => HandshakePacket.Handle(connection, handshakePacket);
                return true;
            }

            default:
                handler = static _ => { };
                return false;
        }
    }
}