using System.Diagnostics.CodeAnalysis;
using Igloo.Buffers;
using Igloo.Network;
using Igloo.Network.Handlers;
using Igloo.Network.Packets;
using Igloo.Players;
using Igloo.Server.Protocol.Outgoing;

namespace Igloo.Server.Protocol;

public class InGameNetworkHandler : INetworkHandler, IPlayerConnection
{
    public bool IsAsync => false;

    private readonly NetworkConnection _connection;

    public InGameNetworkHandler(NetworkConnection connection)
    {
        _connection = connection;
    }

    public bool ReceivePacket(int id, ref BufferReader reader, [NotNullWhen(true)] out IPacketInvoker? invoker)
    {
        invoker = null;
        return false;
    }

    internal void Initialize()
    {
        _connection.Send(new PacketOutDisconnect(
            """
                {
                    "text": "Test",
                    "color": "red"
                }
                """
        ));
    }

    public void Tick(in TimeSpan deltaTime)
    {
    }
}