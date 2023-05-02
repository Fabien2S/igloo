using Igloo.Common.Buffers;

namespace Igloo.Network.Packets;

public interface IWritablePacket
{
    void Serialize(ref BufferWriter writer);
}

public class WritablePacket<T> : IWritablePacket where T : struct, INetworkPacket<T>
{
    private readonly T _packet;

    public WritablePacket(T packet)
    {
        _packet = packet;
    }

    public void Serialize(ref BufferWriter writer)
    {
        writer.WriteVarInt32(T.Id);
        T.Serialize(ref writer, _packet);
    }

    public override string? ToString()
    {
        return _packet.ToString();
    }
}