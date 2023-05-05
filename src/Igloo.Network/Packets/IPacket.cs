using Igloo.Serialization;

namespace Igloo.Network.Packets;

public interface IPacket
{
    static abstract int Id { get; }
}

public interface IPacket<TSelf> : ISerializer<TSelf>, IPacket where TSelf : class, IPacket<TSelf>
{
}