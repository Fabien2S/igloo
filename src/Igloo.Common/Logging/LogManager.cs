using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Igloo.Logging;

public static class LogManager
{
    public static ILoggerFactory Factory { get; set; } = new NullLoggerFactory();

    public static ILogger Create(Type type)
    {
        return Factory.CreateLogger(type);
    }

    public static ILogger<T> Create<T>()
    {
        return Factory.CreateLogger<T>();
    }
}