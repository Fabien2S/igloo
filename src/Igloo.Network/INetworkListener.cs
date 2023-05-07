using Igloo.Profiles;

namespace Igloo.Network;

public interface INetworkListener
{
    void OnPlayerJoined(NetworkConnection socket, GameProfile profile);
}