using Igloo.Logging;
using Igloo.Network;
using Igloo.Players;
using Igloo.Profiles;
using Igloo.Server.Protocol;
using Microsoft.Extensions.Logging;

namespace Igloo.Server;

public class GameServer : INetworkListener
{
    private static readonly ILogger<GameServer> Logger = LogManager.Create<GameServer>();

    private readonly GameServerConfig _config;
    private readonly CancellationTokenSource _cts;

    private readonly NetworkServer _networkServer;
    private readonly PlayerManager _playerManager;

    private bool _running;

    public GameServer(GameServerConfig config)
    {
        _config = config;
        _cts = new CancellationTokenSource();

        _networkServer = new NetworkServer(this);
        _playerManager = new PlayerManager();
    }

    void INetworkListener.OnPlayerJoined(NetworkConnection connection, GameProfile profile)
    {
        var handler = new InGameNetworkHandler(connection);
        connection.SetHandler(handler);

        if (!_playerManager.AddPlayer(profile, handler, out var player))
        {
            return;
        }

        Logger.LogInformation("Player {} joined", player);
        handler.Initialize();
    }

    public async Task<GameServerResult> RunAsync()
    {
        if (_running)
            return GameServerResult.AlreadyStarted;

        try
        {
            _running = true;

            Logger.LogInformation("Starting server");
            Logger.LogDebug("Using {}", _config);

            _networkServer.Listen(_config.EndPoint);

            var tickPeriod = TimeSpan.FromMilliseconds(50);
            var tickTimer = new PeriodicTimer(tickPeriod);
            while (await tickTimer.WaitForNextTickAsync(_cts.Token))
            {
                _networkServer.Tick(in tickPeriod);
                _playerManager.Tick(in tickPeriod);
            }
        }
        catch (Exception e)
        {
            Logger.LogCritical(e, "Server exception");
        }
        finally
        {
            Logger.LogInformation("Shutting down server");

            _networkServer.Close();
            _running = false;
        }

        return GameServerResult.Ok;
    }

    public void Stop()
    {
        _cts.Cancel();
    }
}