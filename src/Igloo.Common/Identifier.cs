using System.Diagnostics;

namespace Igloo.Common;

public readonly struct Identifier : IEquatable<Identifier>, ISpanParsable<Identifier>
{
    public const char Separator = ':';
    public const string DefaultNamespace = "minecraft";

    public ReadOnlySpan<char> Namespace => _key.AsSpan(0, _separator);
    public ReadOnlySpan<char> Path => _key.AsSpan(_separator + 1);

    private readonly string? _key;

    private readonly int _separator;

    private Identifier(IdentifierPart parsed)
    {
        Debug.Assert(IsAllowedNamespace(parsed.Namespace), "parsed.IsNamespaceValid()");
        Debug.Assert(IsAllowedPath(parsed.Path), "parsed.IsPathValid()");

        _key = parsed.ToString();
        _separator = parsed.Namespace.Length;
    }

    public bool Equals(Identifier other)
    {
        return string.Equals(_key, other._key, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        return string.GetHashCode(_key, StringComparison.Ordinal);
    }

    public override string ToString()
    {
        return _key ?? string.Empty;
    }

    public static bool IsAllowedNamespace(char c)
    {
        return char.IsAsciiLetterOrDigit(c) || c == '_' || c == '-' || c == '.';
    }

    public static bool IsAllowedPath(char c)
    {
        return char.IsAsciiLetterOrDigit(c) || c == '_' || c == '-' || c == '.' || c == '/';
    }

    public static bool IsAllowedNamespace(ReadOnlySpan<char> input)
    {
        foreach (var c in input)
        {
            if (IsAllowedNamespace(c))
                continue;

            return false;
        }

        return true;
    }

    public static bool IsAllowedPath(ReadOnlySpan<char> input)
    {
        foreach (var c in input)
        {
            if (IsAllowedPath(c))
                continue;

            return false;
        }

        return true;
    }

    public static Identifier Parse(string input, IFormatProvider? provider)
    {
        return Parse(input.AsSpan(), provider);
    }

    public static bool TryParse(string? input, IFormatProvider? provider, out Identifier result)
    {
        return TryParse(input.AsSpan(), provider, out result);
    }

    public static Identifier Parse(ReadOnlySpan<char> input, IFormatProvider? provider)
    {
        var parsed = Decompose(input);

        if (!IsAllowedNamespace(parsed.Namespace))
            throw new FormatException($"Invalid namespace {parsed.Namespace} (must be [a-z0-9_-.])");

        if (!IsAllowedPath(parsed.Path))
            throw new FormatException($"Invalid namespace {parsed.Namespace} (must be [a-z0-9_-./])");

        return new Identifier(parsed);
    }

    public static bool TryParse(ReadOnlySpan<char> input, IFormatProvider? provider, out Identifier result)
    {
        var parsed = Decompose(input);

        if (!IsAllowedNamespace(parsed.Namespace))
        {
            result = default;
            return false;
        }

        if (!IsAllowedPath(parsed.Path))
        {
            result = default;
            return false;
        }

        result = new Identifier(parsed);
        return true;
    }

    public static IdentifierPart Decompose(ReadOnlySpan<char> input)
    {
        var separatorIndex = input.IndexOf(Separator);
        return new IdentifierPart(
            separatorIndex <= 0 ? DefaultNamespace : input[..separatorIndex],
            input[(separatorIndex + 1)..]
        );
    }
}