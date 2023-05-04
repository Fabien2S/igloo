using Igloo.Logging;
using Igloo.Network;
using Microsoft.Extensions.Logging;

namespace Igloo.Server;

public class GameServer
{
    private static readonly ILogger<GameServer> Logger = LogManager.Create<GameServer>();

    private readonly GameServerConfig _config;
    private readonly CancellationTokenSource _cts;

    private readonly NetworkServer _networkServer;

    private bool _running;

    public GameServer(GameServerConfig config)
    {
        _config = config;
        _cts = new CancellationTokenSource();

        _networkServer = new NetworkServer();
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
            while (await tickTimer.WaitForNextTickAsync(_cts.Token).ConfigureAwait(true))
            {
                _networkServer.Tick(in tickPeriod);
            }
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