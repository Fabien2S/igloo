using System.Net;
using Igloo.Common.Logging;
using Igloo.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

LogManager.Factory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.SetMinimumLevel(LogLevel.Debug);
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = false;
        options.IncludeScopes = true;
        options.ColorBehavior = LoggerColorBehavior.Default;
        options.TimestampFormat = "hh:mm:ss ";
    });
});

var localEndPoint = new IPEndPoint(IPAddress.Loopback, 25565);
var config = new GameServerConfig(localEndPoint);
var server = new GameServer(config);

Console.CancelKeyPress += (_, _) => { server.Stop(); };

var logger = LogManager.Create(typeof(Program));

logger.LogInformation("Starting server...");

var result = server.Run();
if (result != GameServerResult.Ok)
{
    logger.LogError("Server exited with {}", result);
}

logger.LogInformation("Server stopped");