using Igloo.Common.Serialization;

namespace Igloo.Network.Packets;

public interface INetworkPacket<TSelf> : ISerializer<TSelf> where TSelf : struct
{
    static abstract int Id { get; }
}