using Igloo.Common.Buffers;
using Igloo.Network.Packets;

namespace Igloo.Network.Handlers;

public interface INetworkHandler
{
    bool IsAsync { get; }

    bool ReceivePacket(int id, ref BufferReader reader, out PacketHandler handler);
}