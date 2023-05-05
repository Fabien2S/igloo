namespace Igloo.Profiles;

/// <summary>
///     Represents a player game profile.
/// </summary>
/// <param name="Uuid">The id of the player.</param>
/// <param name="Name">The name of the player.</param>
/// <param name="Properties">The properties of the player.</param>
public record struct GameProfile(Guid Uuid, string Name, GameProfile.Property[] Properties)
{
    /// <summary>
    ///     Gets the username minimum length.
    /// </summary>
    public const int UsernameMinLength = 3;

    /// <summary>
    ///     Gets the username maximum length.
    /// </summary>
    public const int UsernameMaxLength = 16;

    /// <summary>
    ///     Indicates whether the specified string is a valid username.
    /// </summary>
    /// <param name="username">The string to test.</param>
    /// <returns>true if <paramref name="username"/> is a valid username; otherwise, false.</returns>
    public static bool IsValidUsername(ReadOnlySpan<char> username)
    {
        if (username.Length is < UsernameMinLength or > UsernameMaxLength)
        {
            return false;
        }

        foreach (var c in username)
        {
            if (c <= 32 || c >= 127)
                return false;
        }

        return true;
    }

    /// <summary>
    ///     Represents a player profile property.
    /// </summary>
    /// <param name="Name">The property name.</param>
    /// <param name="Value">The property value.</param>
    /// <param name="Signature">The property signature, or null if unsigned.</param>
    public record struct Property(string Name, string Value, string? Signature);
}