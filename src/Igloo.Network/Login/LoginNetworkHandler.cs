using System.Diagnostics.CodeAnalysis;
using System.Net;
using Igloo.Buffers;
using Igloo.Logging;
using Igloo.Network.Handlers;
using Igloo.Network.Login.Incoming;
using Igloo.Network.Login.Outgoing;
using Igloo.Network.Packets;
using Igloo.Profiles;
using Microsoft.Extensions.Logging;

namespace Igloo.Network.Login;

public class LoginNetworkHandler : INetworkHandler,
    IPacketHandler<LoginNetworkHandler, PacketInAuthRequest>,
    IPacketHandler<LoginNetworkHandler, PacketInAuthCustomResponse>
{
    private const int AuthTransactionId = 0x00;

    private static readonly ILogger<LoginNetworkHandler> Logger = LogManager.Create<LoginNetworkHandler>();

    public bool IsAsync => true;

    private readonly NetworkConnection _connection;
    private readonly INetworkListener _listener;

    private GameProfile _profile;
    private IPAddress _address;

    private State _state;

    public LoginNetworkHandler(NetworkConnection connection, INetworkListener listener)
    {
        _connection = connection;
        _listener = listener;

        _profile = default;
        _address = IPAddress.None;

        _state = State.VanillaRequest;
    }

    public bool ReceivePacket(int id, ref BufferReader reader, [NotNullWhen(true)] out IPacketInvoker? invoker)
    {
        return id switch
        {
            0x00 => PacketInvoker<LoginNetworkHandler, PacketInAuthRequest>.Read(ref reader, this, out invoker),
            0x02 => PacketInvoker<LoginNetworkHandler, PacketInAuthCustomResponse>.Read(ref reader, this, out invoker),
            _ => (invoker = null) == null
        };
    }

    public void Tick(in TimeSpan deltaTime)
    {
        switch (_state)
        {
            case State.Invalid:
                _connection.Close(NetworkReason.AuthenticationError);
                break;
            case State.Authenticated:
                _listener.OnPlayerJoined(_connection, _profile);
                _state = State.Invalid;
                break;
            case State.VanillaRequest:
            case State.VelocityRequest:
            default:
                break;
        }
    }

    public void Handle(in PacketInAuthRequest packet)
    {
        if (_state != State.VanillaRequest)
        {
            _connection.Close(NetworkReason.AuthenticationError);
            return;
        }

        Logger.LogTrace("Sending auth request to {}", _connection);
        VelocityAuth.SendRequest(_connection, AuthTransactionId);
        _state = State.VelocityRequest;
    }

    public void Handle(in PacketInAuthCustomResponse packet)
    {
        if (_state != State.VelocityRequest)
        {
            _connection.Close(NetworkReason.AuthenticationError);
            return;
        }

        if (packet.TransactionId != AuthTransactionId || packet.Payload == null)
        {
            // The request was not understood by the client
            _connection.Close(NetworkReason.AuthenticationError);
            return;
        }

        var reader = new BufferReader(packet.Payload);
        if (!VelocityAuth.CheckIntegrity(ref reader) || !VelocityAuth.CheckVersion(ref reader))
        {
            // The payload is corrupted
            _connection.Close(NetworkReason.AuthenticationError);
            return;
        }

        if (!VelocityAuth.ReceiveResponse(ref reader, out _address, out _profile))
        {
            // The payload format is incorrect
            _connection.Close(NetworkReason.AuthenticationError);
            return;
        }

        Logger.LogTrace("Received auth request from {}: {} ({})", _connection, _profile, _address);
        _connection.Send(new PacketOutAuthSuccess(_profile));
        _state = State.Authenticated;
    }

    private enum State
    {
        Invalid,
        VanillaRequest,
        VelocityRequest,
        Authenticated
    }
}