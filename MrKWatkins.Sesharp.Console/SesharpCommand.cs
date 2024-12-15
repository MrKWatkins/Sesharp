using System.Reflection;
using MrKWatkins.Sesharp.Markdown.Generation;
using MrKWatkins.Sesharp.Model;
using MrKWatkins.Sesharp.Writerside;
using MrKWatkins.Sesharp.XmlDocumentation;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MrKWatkins.Sesharp.Console;

[UsedImplicitly]
public sealed class DocGenCommand : Command<DocGenSettings>
{
    public override int Execute(CommandContext context, DocGenSettings settings)
    {
        AnsiConsole.MarkupLine($"[green]Loading assembly {settings.AssemblyAbsolutePath}...[/]");
        var assembly = Assembly.LoadFile(settings.AssemblyAbsolutePath);

        var xmlPath = settings.Assembly.Replace(".dll", ".xml", StringComparison.OrdinalIgnoreCase);

        AnsiConsole.MarkupLine($"[green]Loading XML documentation file {xmlPath}...[/]");
        var documentation = Documentation.Load(xmlPath);

        AnsiConsole.MarkupLine("[green]Parsing...[/]");
        var assemblyDetails = AssemblyParser.Parse(assembly, documentation);

        if (settings.DeleteContentsOfOutputDirectory)
        {
            AnsiConsole.MarkupLine($"[green]Deleting existing output directory {settings.OutputDirectoryAbsolutePath}...[/]");
            Directory.Delete(settings.OutputDirectoryAbsolutePath, true);
        }

        AnsiConsole.MarkupLine($"[green]Creating output directory {settings.OutputDirectoryAbsolutePath}...[/]");
        Directory.CreateDirectory(settings.OutputDirectoryAbsolutePath);

        AnsiConsole.MarkupLine("[green]Generating documentation...[/]");
        AssemblyMarkdownGenerator.Generate(assemblyDetails, settings.OutputDirectoryAbsolutePath);

        if (settings.WritersideTreeFileAbsolutePath != null)
        {
            AnsiConsole.MarkupLine($"[green]Updating Writerside tree file {settings.WritersideTreeFileAbsolutePath}...[/]");
            WritersideXmlGenerator.UpdateWriterside(settings, assemblyDetails);
        }

        return 0;
    }
}