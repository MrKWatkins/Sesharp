using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown;

public static class MarkdownGenerator
{
    public static void Generate(Assembly assembly, string outputDirectory)
    {
        var typeLookup = new TypeLookup(assembly.ReflectionAssembly);

        Directory.CreateDirectory(outputDirectory);

        foreach (var @namespace in assembly.Namespaces)
        {
            var namespaceDirectory = Path.Combine(outputDirectory, @namespace.Name);
            Directory.CreateDirectory(namespaceDirectory);

            foreach (var type in @namespace.Types)
            {
                TypeMarkdownGenerator.Generate(typeLookup, namespaceDirectory, type);
            }
        }
    }
}