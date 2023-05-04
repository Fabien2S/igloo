using Igloo.Serialization;

namespace Igloo.Network.Packets;

public interface INetworkPacket<TSelf> : ISerializer<TSelf> where TSelf : struct
{
    static abstract int Id { get; }

    static abstract bool Handle(NetworkConnection connection, in TSelf packet);
}