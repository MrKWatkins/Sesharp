using MrKWatkins.Sesharp.Model;

namespace MrKWatkins.Sesharp.Markdown.Generation;

public static class AssemblyMarkdownGenerator
{
    public static void Generate(IFileSystem fileSystem, AssemblyDetails assemblyDetails, string outputDirectory)
    {
        var memberLookup = new MemberLookup(assemblyDetails.ReflectionAssembly);

        Directory.CreateDirectory(outputDirectory);

        foreach (var @namespace in assemblyDetails.Namespaces)
        {
            // TODO: Namespace file.

            var typeGenerator = new TypeMarkdownGenerator(fileSystem, memberLookup, outputDirectory);

            foreach (var type in @namespace.Types)
            {
                typeGenerator.Generate(type);
            }
        }
    }
}