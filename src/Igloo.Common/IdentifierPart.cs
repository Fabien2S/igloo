namespace Igloo.Common;

public readonly ref struct IdentifierPart
{
    public readonly ReadOnlySpan<char> Namespace;
    public readonly ReadOnlySpan<char> Path;

    public IdentifierPart(ReadOnlySpan<char> @namespace, ReadOnlySpan<char> path)
    {
        Namespace = @namespace;
        Path = path;
    }

    public override string ToString()
    {
        var separatorSpan = new ReadOnlySpan<char>(Identifier.Separator);
        return string.Concat(Namespace, separatorSpan, Path);
    }
}