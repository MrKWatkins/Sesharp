namespace MrKWatkins.Sesharp;

public static class StringExtensions
{
    [Pure]
    public static string Capitalize(this string value) => $"{value[..1].ToUpperInvariant()}{value[1..]}";
}