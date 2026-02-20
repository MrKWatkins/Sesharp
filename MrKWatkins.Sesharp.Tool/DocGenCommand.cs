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
        try
        {
            console.MarkupLine($"[green]Loading assembly {settings.AssemblyAbsolutePath}...[/]");
            var assembly = Assembly.Load(File.ReadAllBytes(settings.AssemblyAbsolutePath));

            var xmlPath = settings.AssemblyAbsolutePath.Replace(".dll", ".xml", StringComparison.OrdinalIgnoreCase);

            console.MarkupLine($"[green]Loading XML documentation file {xmlPath}...[/]");
            var documentation = Documentation.Load(fileSystem, xmlPath);

            console.MarkupLine("[green]Parsing...[/]");
            var assemblyDetails = AssemblyParser.Parse(assembly, documentation);

            if (settings.DeleteContentsOfOutputDirectory)
            {
                console.MarkupLine($"[green]Deleting existing output directory {settings.OutputDirectoryAbsolutePath}...[/]");
                fileSystem.DeleteDirectory(settings.OutputDirectoryAbsolutePath, true);
            }

            console.MarkupLine($"[green]Creating output directory {settings.OutputDirectoryAbsolutePath}...[/]");
            fileSystem.CreateDirectory(settings.OutputDirectoryAbsolutePath);

            console.MarkupLine("[green]Generating documentation...[/]");
            AssemblyMarkdownGenerator.Generate(fileSystem, assemblyDetails, settings.OutputDirectoryAbsolutePath);

            return 0;
        }
        catch (Exception exception)
        {
            console.MarkupLine("[red]Exception generating documentation![/]");
            console.MarkupLine($"[red]{exception}[/]");
            return -1;
        }
    }
}