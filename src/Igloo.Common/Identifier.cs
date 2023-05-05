using System.Diagnostics;
using System.Numerics;

namespace Igloo;

/// <summary>
///     Represents a namespaced-path.
/// </summary>
public readonly struct Identifier : IEquatable<Identifier>, IEqualityOperators<Identifier, Identifier, bool>, ISpanParsable<Identifier>
{
    /// <summary>
    ///     Defines the separator between the namespace and the path
    /// </summary>
    public const char Separator = ':';

    /// <summary>
    ///     Gets the namespace of the identifier
    /// </summary>
    public ReadOnlySpan<char> Namespace => _key.AsSpan(0, _separator);

    /// <summary>
    ///     Gets the path of the identifier
    /// </summary>
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

    /// <inheritdoc />
    public bool Equals(Identifier other)
    {
        return string.Equals(_key, other._key, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Identifier other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return string.GetHashCode(_key, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _key ?? string.Empty;
    }

    private static void Decompose(ReadOnlySpan<char> input, out ReadOnlySpan<char> @namespace, out ReadOnlySpan<char> path)
    {
        var separatorIndex = input.IndexOf(Separator);
        @namespace = separatorIndex <= 0 ? Minecraft.Namespace : input[..separatorIndex];
        path = input[(separatorIndex + 1)..];
    }

    /// <summary>
    ///     Indicates whether the specified character is allowed in a namespace.
    /// </summary>
    /// <param name="c">The character to test.</param>
    /// <returns>true if <paramref name="c"/> is allowed in a namespace; otherwise, false.</returns>
    public static bool IsAllowedNamespace(char c)
    {
        return char.IsAsciiLetterOrDigit(c) || c == '_' || c == '-' || c == '.';
    }

    /// <summary>
    ///     Indicates whether the specified character is allowed in a path.
    /// </summary>
    /// <param name="c">The character to test.</param>
    /// <returns>true if <paramref name="c"/> is allowed in a path; otherwise, false.</returns>
    public static bool IsAllowedPath(char c)
    {
        return char.IsAsciiLetterOrDigit(c) || c == '_' || c == '-' || c == '.' || c == '/';
    }

    /// <summary>
    ///     Indicates whether the specified string is a valid namespace.
    /// </summary>
    /// <param name="input">The string to test.</param>
    /// <returns>true if <paramref name="input"/> is a valid namespace; otherwise, false.</returns>
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

    /// <summary>
    ///     Indicates whether the specified string is a valid path.
    /// </summary>
    /// <param name="input">The string to test.</param>
    /// <returns>true if <paramref name="input"/> is a valid path; otherwise, false.</returns>
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

    /// <summary>
    ///     Creates a new <see cref="Identifier"/> with a specific namespace and path.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="path">The path.</param>
    /// <returns>An <see cref="Identifier"/> created from the <paramref name="namespace"/> and <paramref name="path"/>.</returns>
    /// <exception cref="FormatException"><paramref name="namespace"/> and/or <paramref name="path"/> are not in the correct format.</exception>
    /// <seealso cref="IsAllowedNamespace(ReadOnlySpan{char})"/>
    /// <seealso cref="IsAllowedPath(ReadOnlySpan{char})"/>
    public static Identifier Create(ReadOnlySpan<char> @namespace, ReadOnlySpan<char> path)
    {
        if (!IsAllowedNamespace(@namespace))
            throw new FormatException($"Invalid namespace {@namespace} (must be [a-z0-9_-.])");

        if (!IsAllowedPath(path))
            throw new FormatException($"Invalid path {path} (must be [a-z0-9_-./])");

        return new Identifier(@namespace, path);
    }

    /// <inheritdoc cref="Parse(ReadOnlySpan{char},System.IFormatProvider?)"/>
    public static Identifier Parse(string input, IFormatProvider? provider)
    {
        return Parse(input.AsSpan(), provider);
    }

    /// <inheritdoc cref="TryParse(ReadOnlySpan{char},System.IFormatProvider?,out Igloo.Identifier)"/>
    public static bool TryParse(string? input, IFormatProvider? provider, out Identifier result)
    {
        return TryParse(input.AsSpan(), provider, out result);
    }

    /// <summary>
    ///     Creates an <see cref="Identifier"/> from its string representation.
    /// </summary>
    /// <param name="input">The string representation of an <see cref="Identifier"/>.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information about <paramref name="input"/>.</param>
    /// <returns>An <see cref="Identifier"/> created from <paramref name="input"/>.</returns>
    public static Identifier Parse(ReadOnlySpan<char> input, IFormatProvider? provider)
    {
        Decompose(input, out var @namespace, out var path);
        return Create(@namespace, path);
    }

    /// <summary>
    ///     Tries to create an <see cref="Identifier"/> from its string representation.
    /// </summary>
    /// <param name="input">The string representation of an <see cref="Identifier"/>.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information about <paramref name="input"/>.</param>
    /// <param name="result">The successfully parsed <see cref="Identifier"/>, or an undefined value on failure.</param>
    /// <returns>true if <paramref name="input"/> was successfully parsed; otherwise, false.</returns>
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

    /// <inheritdoc />
    public static bool operator ==(Identifier left, Identifier right)
    {
        return left.Equals(right);
    }

    /// <inheritdoc />
    public static bool operator !=(Identifier left, Identifier right)
    {
        return !(left == right);
    }
}