using System.ComponentModel;
using Spectre.Console.Cli;

namespace MrKWatkins.Sesharp.Tool;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class DocGenSettings : CommandSettings
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

    [CommandOption("--repository")]
    public string? Repository { get; init; }
}