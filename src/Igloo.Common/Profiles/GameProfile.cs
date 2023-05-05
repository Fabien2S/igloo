namespace Igloo.Profiles;

public record struct GameProfile(Guid Uuid, string Name, GameProfile.Property[] Properties)
{
    public const int UsernameMinLength = 3;
    public const int UsernameMaxLength = 16;

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

    public record struct Property(string Name, string Value, string? Signature);
}