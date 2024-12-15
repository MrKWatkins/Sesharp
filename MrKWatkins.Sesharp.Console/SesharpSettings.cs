using System.ComponentModel;
using MrKWatkins.Sesharp.Writerside;
using Spectre.Console.Cli;

namespace MrKWatkins.Sesharp.Console;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class DocGenSettings : CommandSettings, IWritersideOptions
{
    [CommandArgument(0, "<assembly>")]
    public string Assembly { get; init; } = null!;

    public string AssemblyAbsolutePath => Path.GetFullPath(Assembly);

    [CommandArgument(1, "<outputDirectory>")]
    public string OutputDirectory { get; init; } = null!;

    public string OutputDirectoryAbsolutePath => Path.GetFullPath(OutputDirectory);

    [CommandOption("--delete-contents-of-output-directory")]
    [DefaultValue(true)]
    public bool DeleteContentsOfOutputDirectory { get; init; }

    [CommandOption("--writerside-tree-file")]
    public string? WritersideTreeFile { get; init; }

    public string? WritersideTreeFileAbsolutePath => WritersideTreeFile != null ? Path.GetFullPath(WritersideTreeFile) : WritersideTreeFile;

    string IWritersideOptions.TreeFile => WritersideTreeFileAbsolutePath!;

    [CommandOption("--writerside-toc-element-id")]
    [DefaultValue("API")]
    public string WritersideTocElementId { get; init; } = null!;

    string IWritersideOptions.TocElementId => WritersideTocElementId;

    [CommandOption("--writerside-toc-element-title")]
    [DefaultValue("API")]
    public string WritersideTocElementTitle { get; init; } = null!;

    string IWritersideOptions.TocElementTitle => WritersideTocElementTitle;
}