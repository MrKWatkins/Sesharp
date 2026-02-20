namespace MrKWatkins.Sesharp.Markdown;

public sealed class MarkdownOptions : IMarkdownOptions
{
    public MarkdownIdFormat IdFormat { get; init; } = MarkdownIdFormat.MkDocs;
}

