using System.Diagnostics.CodeAnalysis;

namespace Igloo.Extensions;

public static class NullableExtensions
{
    public static bool TryGetValue<T>(this T? nullable, [NotNullWhen(true)] out T value) where T : struct
    {
        value = nullable.GetValueOrDefault();
        return nullable.HasValue;
    }
}