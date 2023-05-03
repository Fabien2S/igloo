using Igloo.Common.Buffers;
using Igloo.Network.Handlers;
using Igloo.Network.Packets;

namespace Igloo.Network.Login;

public class LoginNetworkHandler : INetworkHandler
{
    public bool IsAsync => true;

    private State _state;

    private enum State
    {
    }

    public bool ReceivePacket(int id, ref BufferReader reader, out PacketHandler handler)
    {
        handler = static _ => { };
        return true;
    }
}