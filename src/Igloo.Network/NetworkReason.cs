namespace Igloo.Network;

public enum NetworkReason
{
    Ok,
    ClosedRemotely,
    ClosedLocally,
    SocketError,
    ProtocolError,
}