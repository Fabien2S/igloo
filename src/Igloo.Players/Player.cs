using Igloo.Profiles;
using Igloo.Timings;

namespace Igloo.Players;

public class Player : ITickable
{
    public GameProfile Profile { get; }
    public IPlayerConnection Connection { get; }

    public Player(GameProfile profile, IPlayerConnection connection)
    {
        Profile = profile;
        Connection = connection;
    }

    public void Tick(in TimeSpan deltaTime)
    {
    }

    public override string ToString()
    {
        return $"{Profile.Name} {Profile.Uuid:P}";
    }
}