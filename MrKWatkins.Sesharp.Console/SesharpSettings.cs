using System.ComponentModel;
using MrKWatkins.Sesharp.Writerside;
using Spectre.Console.Cli;

namespace MrKWatkins.Sesharp.Console;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class DocGenSettings : CommandSettings, IWritersideOptions
{
    [CommandArgument(0, "<assembly>")]
    public string Assembly { get; init; } = null!;

    [CommandArgument(1, "<assemblyPath>")]
    public string OutputDirectory { get; init; } = null!;

    [CommandOption("--delete-contents-of-output-directory")]
    [DefaultValue(true)]
    public bool DeleteContentsOfOutputDirectory { get; init; }

    [CommandOption("--writerside-tree-file")]
    public string? WritersideTreeFile { get; init; }

    string IWritersideOptions.TreeFile => WritersideTreeFile!;

    [CommandOption("--writerside-toc-element-id")]
    [DefaultValue("API")]
    public string WritersideTocElementId { get; init; } = null!;

    string IWritersideOptions.TocElementId => WritersideTocElementId;

    [CommandOption("--writerside-toc-element-title")]
    [DefaultValue("API")]
    public string WritersideTocElementTitle { get; init; } = null!;

    string IWritersideOptions.TocElementTitle => WritersideTocElementTitle;
}