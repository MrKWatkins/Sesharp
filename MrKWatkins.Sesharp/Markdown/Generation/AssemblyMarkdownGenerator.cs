using MrKWatkins.Sesharp.Model;

namespace MrKWatkins.Sesharp.Markdown.Generation;

public static class AssemblyMarkdownGenerator
{
    public static void Generate(IFileSystem fileSystem, IReadOnlyList<AssemblyDetails> assembliesDetails, string outputDirectory, string? repositoryUrl = null)
    {
        var memberLookup = new MemberLookup(assembliesDetails.Select(a => a.ReflectionAssembly));

        fileSystem.CreateDirectory(outputDirectory);

        foreach (var assemblyDetails in assembliesDetails)
        {
            GenerateAssembly(fileSystem, memberLookup, assemblyDetails, outputDirectory, repositoryUrl);
        }
    }

    public static void Generate(IFileSystem fileSystem, AssemblyDetails assemblyDetails, string outputDirectory, string? repositoryUrl = null)
        => Generate(fileSystem, [assemblyDetails], outputDirectory, repositoryUrl);

    private static void GenerateAssembly(IFileSystem fileSystem, MemberLookup memberLookup, AssemblyDetails assemblyDetails, string outputDirectory, string? repositoryUrl)
    {
        foreach (var @namespace in assemblyDetails.Namespaces)
        {
            // TODO: Namespace file.

            var typeGenerator = new TypeMarkdownGenerator(fileSystem, memberLookup, outputDirectory);
            typeGenerator.RepositoryUrl = repositoryUrl;

            foreach (var type in @namespace.Types)
            {
                var typeDir = Path.Combine(outputDirectory, type.MemberInfo.BuildTypeDirectory());
                fileSystem.CreateDirectory(typeDir);

                typeGenerator.Generate(type);
            }
        }
    }
}