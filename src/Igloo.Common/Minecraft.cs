namespace Igloo;

/// <summary>
///     Represents all Minecraft-related values
/// </summary>
public static class Minecraft
{
    /// <summary>
    ///     Gets the Minecraft namespace.
    /// </summary>
    public const string Namespace = "minecraft";

    /// <summary>
    ///     Creates a Minecraft <see cref="Identifier"/>.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>The Minecraft identifier.</returns>
    public static Identifier Id(ReadOnlySpan<char> path)
    {
        return Identifier.Create(Namespace, path);
    }
}