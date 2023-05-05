using Igloo.Buffers;
using Igloo.Serialization;

namespace Igloo.Network.Packets;

public interface IPacketOut
{
    void Serialize(ref BufferWriter writer);
}

public interface IPacketOut<TSelf> : IPacket<TSelf>, IPacketOut where TSelf : class, IPacketOut<TSelf>
{
    void IPacketOut.Serialize(ref BufferWriter writer)
    {
        writer.WriteVarInt(TSelf.Id);
        TSelf.Serialize(ref writer, (TSelf)this);
    }

    static void ISerializer<TSelf>.Deserialize(ref BufferReader reader, out TSelf value)
    {
        throw new NotSupportedException("Deserializing an outgoing packet is not supported");
    }
}