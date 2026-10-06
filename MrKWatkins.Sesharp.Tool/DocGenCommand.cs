using System.Reflection;
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
        // Assemblies loaded from bytes aren't registered by name in the default load context.
        // This handler resolves them when another assembly references them by name.
        AppDomain.CurrentDomain.AssemblyResolve += ResolveFromLoaded;

        try
        {
            var assembliesDetails = new List<AssemblyDetails>();

            foreach (var assemblyPath in settings.AssemblyAbsolutePaths)
            {
                console.MarkupLine($"[green]Loading assembly {assemblyPath}...[/]");
                var assembly = Assembly.Load(File.ReadAllBytes(assemblyPath));

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
            AssemblyMarkdownGenerator.Generate(fileSystem, assembliesDetails, settings.OutputDirectoryAbsolutePath, settings.Repository);

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
            AppDomain.CurrentDomain.AssemblyResolve -= ResolveFromLoaded;
        }
    }

    private static Assembly? ResolveFromLoaded(object? sender, ResolveEventArgs args)
    {
        var name = new AssemblyName(args.Name);
        return AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => AssemblyName.ReferenceMatchesDefinition(name, a.GetName()));
    }
}