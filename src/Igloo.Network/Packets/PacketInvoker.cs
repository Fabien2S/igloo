using Igloo.Buffers;

namespace Igloo.Network.Packets;

public interface IPacketInvoker
{
    void Invoke();
}

public record PacketInvoker<THandler, TPacket>(THandler Handler, TPacket Packet) : IPacketInvoker
    where THandler : IPacketHandler<THandler, TPacket>
    where TPacket : class, IPacketIn<TPacket, THandler>
{
    public void Invoke()
    {
        Handler.Handle(Packet);
    }

    public static bool Read(ref BufferReader reader, THandler handler, out IPacketInvoker invoker)
    {
        TPacket.Deserialize(ref reader, out var packet);
        invoker = new PacketInvoker<THandler, TPacket>(handler, packet);
        return true;
    }
}