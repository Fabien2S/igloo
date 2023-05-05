using Igloo.Buffers;
using Igloo.Serialization;

namespace Igloo.Network.Packets;

public interface IPacketIn<TSelf, THandler> : IPacket<TSelf>
    where THandler : IPacketHandler<THandler, TSelf>
    where TSelf : class, IPacketIn<TSelf, THandler>
{
    static void ISerializer<TSelf>.Serialize(ref BufferWriter writer, in TSelf value)
    {
        throw new NotSupportedException("Serializing an incoming packet is not supported");
    }
}