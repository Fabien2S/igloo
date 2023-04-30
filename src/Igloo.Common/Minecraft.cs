namespace Igloo.Common;

public static class Minecraft
{
    public const string Namespace = "minecraft";

    public static Identifier Id(ReadOnlySpan<char> path)
    {
        return Identifier.Create(Namespace, path);
    }
}