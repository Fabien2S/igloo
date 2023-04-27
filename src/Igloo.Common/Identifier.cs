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

    private Identifier(ReadOnlySpan<char> @namespace, ReadOnlySpan<char> path)
    {
        Debug.Assert(IsAllowedNamespace(@namespace), "IsAllowedNamespace(namespace)");
        Debug.Assert(IsAllowedPath(path), "IsAllowedPath(path)");

        var separatorSpan = new ReadOnlySpan<char>(Separator);
        _key = string.Concat(@namespace, separatorSpan, path);
        _separator = @namespace.Length;
    }

    public bool Equals(Identifier other)
    {
        return string.Equals(_key, other._key, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is Identifier other && Equals(other);
    }

    public override int GetHashCode()
    {
        return string.GetHashCode(_key, StringComparison.Ordinal);
    }

    public override string ToString()
    {
        return _key ?? string.Empty;
    }

    private static void Decompose(ReadOnlySpan<char> input, out ReadOnlySpan<char> @namespace, out ReadOnlySpan<char> path)
    {
        var separatorIndex = input.IndexOf(Separator);
        @namespace = separatorIndex <= 0 ? DefaultNamespace : input[..separatorIndex];
        path = input[(separatorIndex + 1)..];
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
        Decompose(input, out var @namespace, out var path);

        if (!IsAllowedNamespace(@namespace))
            throw new FormatException($"Invalid namespace {@namespace} (must be [a-z0-9_-.])");

        if (!IsAllowedPath(path))
            throw new FormatException($"Invalid path {path} (must be [a-z0-9_-./])");

        return new Identifier(@namespace, path);
    }

    public static bool TryParse(ReadOnlySpan<char> input, IFormatProvider? provider, out Identifier result)
    {
        Decompose(input, out var @namespace, out var path);

        if (!IsAllowedNamespace(@namespace))
        {
            result = default;
            return false;
        }

        if (!IsAllowedPath(path))
        {
            result = default;
            return false;
        }

        result = new Identifier(@namespace, path);
        return true;
    }

    public static bool operator ==(Identifier left, Identifier right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Identifier left, Identifier right)
    {
        return !(left == right);
    }
}