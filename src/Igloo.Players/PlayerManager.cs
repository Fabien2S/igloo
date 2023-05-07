using Igloo.Profiles;
using Igloo.Timings;

namespace Igloo.Players;

public class PlayerManager : ITickable
{
    private readonly HashSet<Player> _players = new();

    public void Tick(in TimeSpan deltaTime)
    {
        foreach (var player in _players)
            player.Tick(in deltaTime);
    }

    public bool AddPlayer(GameProfile profile, IPlayerConnection connection, out Player player)
    {
        player = new Player(profile, connection);
        return _players.Add(player);
    }
}