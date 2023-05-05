using System.Diagnostics.CodeAnalysis;
using Igloo.Buffers;
using Igloo.Network.Handlers;
using Igloo.Network.Login;
using Igloo.Network.Packets;

namespace Igloo.Network.Handshake;

public class HandshakeNetworkHandler : INetworkHandler, IPacketHandler<HandshakeNetworkHandler, PacketInHandshake>
{
    public bool IsAsync => true;

    private readonly NetworkConnection _connection;

    public HandshakeNetworkHandler(NetworkConnection connection)
    {
        _connection = connection;
    }

    public bool ReceivePacket(int id, ref BufferReader reader, [NotNullWhen(true)] out IPacketInvoker? invoker)
    {
        return id switch
        {
            0x00 => PacketInvoker<HandshakeNetworkHandler, PacketInHandshake>.Read(ref reader, this, out invoker),
            _ => (invoker = null) == null
        };
    }

    public void Handle(in PacketInHandshake packet)
    {
        switch (packet.RequestedState)
        {
            case PacketInHandshake.State.Login:
                _connection.Handler = new LoginNetworkHandler(_connection);
                return;
            case PacketInHandshake.State.Handshake:
            case PacketInHandshake.State.Status:
            default:
                _connection.Close(NetworkReason.ClosedLocally);
                break;
        }
    }
}