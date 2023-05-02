using Igloo.Common.Logging;
using Igloo.Network;
using Microsoft.Extensions.Logging;

namespace Igloo.Server;

public class GameServer
{
    private static readonly ILogger<GameServer> Logger = LogManager.Create<GameServer>();

    private readonly GameServerConfig _config;
    private readonly NetworkServer _networkServer;

    private bool _running;

    public GameServer(GameServerConfig config)
    {
        _config = config;
        _networkServer = new NetworkServer();
    }

    public GameServerResult Run()
    {
        if (_running)
            return GameServerResult.AlreadyStarted;

        try
        {
            _running = true;

            _networkServer.Listen(_config.EndPoint);

            while (_running)
            {
                Thread.Yield();
            }
        }
        finally
        {
            _networkServer.Close();
        }

        return GameServerResult.Ok;
    }

    public void Stop()
    {
        _running = false;
    }
}