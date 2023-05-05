using System.Diagnostics.CodeAnalysis;
using Igloo.Buffers;
using Igloo.Network.Packets;

namespace Igloo.Network.Handlers;

public interface INetworkHandler
{
    bool IsAsync { get; }

    bool ReceivePacket(int id, ref BufferReader reader, [NotNullWhen(true)] out IPacketInvoker? invoker);
}