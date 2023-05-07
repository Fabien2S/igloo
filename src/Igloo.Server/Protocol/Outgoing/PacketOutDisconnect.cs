using Igloo.Buffers;
using Igloo.Network.Packets;

namespace Igloo.Server.Protocol.Outgoing;

public record PacketOutDisconnect(string Json) : IPacketOut<PacketOutDisconnect>
{
    public static int Id => 0x1A;

    public static void Serialize(ref BufferWriter writer, in PacketOutDisconnect value)
    {
        writer.WriteString(value.Json);
    }
}