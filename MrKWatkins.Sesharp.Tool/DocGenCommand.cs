using System.Reflection;
using System.Runtime.Loader;
using MrKWatkins.Sesharp.Markdown.Generation;
using MrKWatkins.Sesharp.Model;
using MrKWatkins.Sesharp.XmlDocumentation;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MrKWatkins.Sesharp.Tool;

[UsedImplicitly]
public sealed class DocGenCommand(IAnsiConsole console, IFileSystem fileSystem) : Command<DocGenSettings>
{
    public override int Execute(CommandContext context, DocGenSettings settings, CancellationToken cancellationToken)
    {
        // Documented dependencies can use different versions from Sesharp's own packages.
        var loadContext = new DocumentationLoadContext(settings.AssemblyAbsolutePaths);

        try
        {
            var assembliesDetails = new List<AssemblyDetails>();

            foreach (var assemblyPath in settings.AssemblyAbsolutePaths)
            {
                console.MarkupLine($"[green]Loading assembly {assemblyPath}...[/]");
                var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);

                var xmlPath = assemblyPath.Replace(".dll", ".xml", StringComparison.OrdinalIgnoreCase);

                console.MarkupLine($"[green]Loading XML documentation file {xmlPath}...[/]");
                var documentation = Documentation.Load(fileSystem, xmlPath);

                console.MarkupLine("[green]Parsing...[/]");
                assembliesDetails.Add(AssemblyParser.Parse(assembly, documentation, assemblyPath));
            }

            if (settings.DeleteContentsOfOutputDirectory && fileSystem.DirectoryExists(settings.OutputDirectoryAbsolutePath))
            {
                console.MarkupLine($"[green]Deleting existing output directory {settings.OutputDirectoryAbsolutePath}...[/]");
                fileSystem.DeleteDirectory(settings.OutputDirectoryAbsolutePath, true);
            }

            fileSystem.CreateDirectory(settings.OutputDirectoryAbsolutePath);

            console.MarkupLine("[green]Generating documentation...[/]");
            AssemblyMarkdownGenerator.Generate(fileSystem, assembliesDetails, settings.OutputDirectoryAbsolutePath, settings.Repository, settings.Dialect, settings.SourceRef);

            return 0;
        }
        catch (Exception exception)
        {
            console.MarkupLine("[red]Exception generating documentation![/]");
            console.MarkupLine($"[red]{exception.ToString().EscapeMarkup()}[/]");
            return -1;
        }
        finally
        {
            loadContext.Unload();
        }
    }

    private sealed class DocumentationLoadContext(IEnumerable<string> assemblyPaths)
        : AssemblyLoadContext(nameof(DocumentationLoadContext), isCollectible: true)
    {
        private readonly string[] directories = assemblyPaths.Select(path => Path.GetDirectoryName(path)!).Distinct().ToArray();

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            foreach (var directory in directories)
            {
                var path = Path.Combine(directory, $"{assemblyName.Name}.dll");
                if (File.Exists(path))
                {
                    return LoadFromAssemblyPath(path);
                }
            }

            // Framework assemblies remain shared with the runtime.
            return null;
        }
    }
}