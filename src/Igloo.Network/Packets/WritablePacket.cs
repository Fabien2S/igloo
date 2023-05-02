using System.Buffers;
using Igloo.Common.Buffers;

namespace Igloo.Network.Packets;

public interface IWritablePacket
{
    void Serialize(IBufferWriter<byte> buffer);
}

public class WritablePacket<T> : IWritablePacket where T : struct, INetworkPacket<T>
{
    private readonly T _packet;

    public WritablePacket(T packet)
    {
        _packet = packet;
    }

    public void Serialize(IBufferWriter<byte> buffer)
    {
        var writer = new BufferWriter(buffer);
        writer.WriteVarInt32(T.Id);
        T.Serialize(ref writer, _packet);
    }

    public override string? ToString()
    {
        return _packet.ToString();
    }
}