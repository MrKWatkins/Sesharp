namespace MrKWatkins.DocGen.Markdown;

public static class TypeMarkdown
{
    public static void Generate(string namespaceDirectory, Model.Type type)
    {
        var filePath = Path.Combine(namespaceDirectory, type.FileName);
        using var writer = new MarkdownWriter(filePath + ".md");

        writer.WriteMainHeading($"{type.DisplayName} {type.Kind.Capitalize()}");
    }
}