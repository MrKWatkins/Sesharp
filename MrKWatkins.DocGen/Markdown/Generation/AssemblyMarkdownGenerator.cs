using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

public static class AssemblyMarkdownGenerator
{
    public static void Generate(Assembly assembly, string outputDirectory)
    {
        var typeLookup = new TypeLookup(assembly.ReflectionAssembly);

        Directory.CreateDirectory(outputDirectory);

        foreach (var @namespace in assembly.Namespaces)
        {
            // TODO: Namespace file.
            var namespaceDirectory = Path.Combine(outputDirectory, @namespace.Name);
            Directory.CreateDirectory(namespaceDirectory);

            var typeGenerator = new TypeMarkdownGenerator(typeLookup, namespaceDirectory);

            foreach (var type in @namespace.Types)
            {
                typeGenerator.Generate(type);
            }
        }
    }
}