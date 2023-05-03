using Igloo.Common.Buffers;

namespace Igloo.Network.Packets;

public delegate void PacketHandler(NetworkConnection connection);

public delegate void PacketSerializer(ref BufferWriter writer);