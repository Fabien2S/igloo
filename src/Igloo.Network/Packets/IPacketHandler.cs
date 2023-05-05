namespace Igloo.Network.Packets;

public interface IPacketHandler<TSelf, TPacket>
    where TSelf : IPacketHandler<TSelf, TPacket>
    where TPacket : class, IPacketIn<TPacket, TSelf>
{
    void Handle(in TPacket packet);
}