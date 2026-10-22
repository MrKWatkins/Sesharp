using System.Text.Json;
using MrKWatkins.Sesharp.Markdown.Writing;
using MrKWatkins.Sesharp.Model;

namespace MrKWatkins.Sesharp.Markdown.Generation;

public static class AssemblyMarkdownGenerator
{
    private static readonly JsonSerializerOptions NavigationJsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static void Generate(IFileSystem fileSystem, IReadOnlyList<AssemblyDetails> assembliesDetails, string outputDirectory,
        string? repositoryUrl = null, MarkdownDialect dialect = MarkdownDialect.VitePress, string sourceRef = "main")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceRef);
        if (!Enum.IsDefined(dialect))
        {
            throw new ArgumentOutOfRangeException(nameof(dialect));
        }

        var memberLookup = new MemberLookup(assembliesDetails.Select(a => a.ReflectionAssembly));
        var navigation = new List<NavigationEntry>();
        var namespaces = assembliesDetails.SelectMany(a => a.Namespaces)
            .GroupBy(n => n.Name).OrderBy(g => g.Key, StringComparer.Ordinal).ToList();

        fileSystem.CreateDirectory(outputDirectory);
        using (var index = new MarkdownWriter(fileSystem, Path.Combine(outputDirectory, "index.md"), dialect))
        {
            index.WriteFrontMatter("API");
            index.WriteMainHeading("API");
            foreach (var group in namespaces)
            {
                using var paragraph = index.Paragraph();
                paragraph.WriteLink(group.Key, $"{group.Key}/index.md");
            }
        }

        foreach (var group in namespaces)
        {
            var types = group.SelectMany(n => n.Types).OrderBy(t => t.FileName, StringComparer.Ordinal).ToList();
            var collision = types.GroupBy(t => t.FileName).FirstOrDefault(g => g.Count() > 1);
            if (collision != null)
            {
                throw new InvalidOperationException($"Multiple types would write {collision.Key}; use assemblies with distinct public type names.");
            }

            var namespacePath = $"{group.Key}/index.md";
            fileSystem.CreateDirectory(Path.Combine(outputDirectory, group.Key));
            // The table borrows this writer; it does not take ownership.
#pragma warning disable CA2000
            using (var writer = new MarkdownWriter(fileSystem, Path.Combine(outputDirectory, namespacePath), dialect))
#pragma warning restore CA2000
            {
                writer.WriteFrontMatter(group.Key);
                writer.WriteMainHeading($"{group.Key} Namespace");
                using var table = writer.Table("Type", "Kind");
                foreach (var type in types)
                {
                    table.NewRow();
                    table.WriteCode(type.DisplayName, Path.GetRelativePath(group.Key, type.FileName).Replace('\\', '/'));
                    table.NewColumn();
                    table.Write(type.Kind);
                }
            }
            navigation.Add(new NavigationEntry($"N:{group.Key}", namespacePath, group.Key, "namespace", null));

            var generator = new TypeMarkdownGenerator(fileSystem, memberLookup, outputDirectory)
            {
                RepositoryUrl = repositoryUrl,
                SourceRef = sourceRef,
                Dialect = dialect,
                Navigation = navigation
            };
            foreach (var type in types)
            {
                fileSystem.CreateDirectory(Path.Combine(outputDirectory, type.MemberInfo.BuildTypeDirectory()));
                generator.Generate(type);
            }
        }

        using var manifest = fileSystem.CreateText(Path.Combine(outputDirectory, "navigation.json"));
        manifest.WriteLine(JsonSerializer.Serialize(new
        {
            schemaVersion = 1,
            entries = navigation.OrderBy(e => e.Path, StringComparer.Ordinal).ThenBy(e => e.Id, StringComparer.Ordinal)
        }, NavigationJsonOptions));
    }

    public static void Generate(IFileSystem fileSystem, AssemblyDetails assemblyDetails, string outputDirectory,
        string? repositoryUrl = null, MarkdownDialect dialect = MarkdownDialect.VitePress, string sourceRef = "main")
        => Generate(fileSystem, [assemblyDetails], outputDirectory, repositoryUrl, dialect, sourceRef);
}