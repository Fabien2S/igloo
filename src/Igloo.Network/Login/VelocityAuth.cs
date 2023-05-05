using System.Net;
using System.Security.Cryptography;
using System.Text;
using Igloo.Buffers;
using Igloo.Network.Login.Outgoing;
using Igloo.Profiles;

namespace Igloo.Network.Login;

internal static class VelocityAuth
{
    private static readonly Identifier PlayerInfoChannel = Identifier.Create("velocity", "player_info");
    private static readonly byte[] PlayerInfoPayload = { (byte)ProtocolVersion.LazySession };

    private static readonly byte[] Secret;

    static VelocityAuth()
    {
        var secret = Environment.GetEnvironmentVariable("SECRET");
        Secret = string.IsNullOrEmpty(secret) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(secret);
    }

    public static void SendRequest(NetworkConnection connection, int transactionId)
    {
        connection.Send(new PacketOutAuthCustomRequest(transactionId, PlayerInfoChannel, PlayerInfoPayload));
    }

    public static bool CheckIntegrity(ref BufferReader reader)
    {
        var signature = reader.ReadBytes(HMACSHA256.HashSizeInBytes);
        var destination = (Span<byte>)stackalloc byte[HMACSHA256.HashSizeInBytes];
        return HMACSHA256.TryHashData(Secret, reader.RemainingSpan, destination, out _) && signature.SequenceEqual(destination);
    }

    public static bool CheckVersion(ref BufferReader reader)
    {
        var protocol = (ProtocolVersion)reader.ReadVarInt();
        return protocol == ProtocolVersion.LazySession;
    }

    public static bool ReceiveResponse(ref BufferReader reader, out IPAddress address, out GameProfile profile)
    {
        // read ip
        var addrStr = reader.ReadString();
        if (!IPAddress.TryParse(addrStr, out var addr))
        {
            address = IPAddress.None;
            profile = default;
            return false;
        }

        address = addr;

        // read profile
        var uuid = reader.ReadUuid();
        var username = reader.ReadString(GameProfile.UsernameMaxLength);

        var propertyCount = reader.ReadVarInt();
        var properties = new GameProfile.Property[propertyCount];
        for (var i = 0; i < propertyCount; i++)
        {
            properties[i] = new GameProfile.Property(
                reader.ReadString(),
                reader.ReadString(),
                reader.ReadBool() ? reader.ReadString() : null
            );
        }

        profile = new GameProfile(uuid, username, properties);
        return true;
    }

    private enum ProtocolVersion : byte
    {
        Default = 1,
        WithKey,
        WithKeyV2,
        LazySession
    }
}