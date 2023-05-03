using System.Net;
using Igloo.Common.Logging;
using Igloo.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

LogManager.Factory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.SetMinimumLevel(LogLevel.Trace);
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

return (int)await server.RunAsync();