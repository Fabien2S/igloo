using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Igloo.Logging;

public static class LogManager
{
    /// <summary>
    ///     Gets or sets the factory used to create the loggers.
    /// </summary>
    public static ILoggerFactory Factory { get; set; } = new NullLoggerFactory();

    /// <summary>
    ///     Creates a logger of type <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type of the logger.</param>
    /// <returns>The created logger.</returns>
    public static ILogger Create(Type type)
    {
        return Factory.CreateLogger(type);
    }

    /// <summary>
    ///     Creates a logger of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the logger.</typeparam>
    /// <returns>The created logger.</returns>
    public static ILogger<T> Create<T>()
    {
        return Factory.CreateLogger<T>();
    }
}