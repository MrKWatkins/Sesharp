using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown;

public static class MarkdownGenerator
{
    public static void Generate(Assembly assembly, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        foreach (var @namespace in assembly.Namespaces)
        {
            var namespaceDirectory = Path.Combine(outputDirectory, @namespace.Name);
            Directory.CreateDirectory(namespaceDirectory);

            foreach (var type in @namespace.Types)
            {
                TypeMarkdown.Generate(namespaceDirectory, type);
            }
        }
    }
}