using System.Text.Json;

namespace MrKWatkins.Sesharp.SourceLink;

internal sealed class SourceLinkMap
{
    private readonly IReadOnlyList<string> pathPrefixes;

    private SourceLinkMap(IReadOnlyList<string> pathPrefixes)
    {
        this.pathPrefixes = pathPrefixes;
    }

    internal static SourceLinkMap? TryParse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("documents", out var documents))
                return null;

            var prefixes = new List<string>();

            foreach (var prop in documents.EnumerateObject())
            {
                var pattern = prop.Name.Replace('\\', '/');

                // Pattern ends with /* — strip the trailing /*
                var prefix = pattern.EndsWith("/*", StringComparison.Ordinal) ? pattern[..^2] : pattern;

                prefixes.Add(prefix);
            }

            return prefixes.Count > 0 ? new SourceLinkMap(prefixes) : null;
        }
        catch
        {
            return null;
        }
    }

    internal string? TryMap(string absolutePath)
    {
        var normalised = absolutePath.Replace('\\', '/');

        foreach (var prefix in pathPrefixes)
        {
            if (normalised.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return normalised[prefix.Length..].TrimStart('/');
        }

        return null;
    }
}