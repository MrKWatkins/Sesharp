using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MrKWatkins.Sesharp.Tool;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class DocGenSettings : CommandSettings
{
    [CommandArgument(0, "<assemblies>")]
    public string[] Assemblies { get; init; } = null!;

    public IReadOnlyList<string> AssemblyAbsolutePaths => Assemblies.Select(Path.GetFullPath).ToList();

    [CommandOption("-o|--output")]
    public string? OutputDirectory { get; init; }

    public string OutputDirectoryAbsolutePath => Path.GetFullPath(OutputDirectory!);

    [CommandOption("--delete-contents-of-output-directory")]
    [DefaultValue(true)]
    public bool DeleteContentsOfOutputDirectory { get; init; }

    [CommandOption("-r|--repository")]
    public string? Repository { get; init; }

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(OutputDirectory))
        {
            return ValidationResult.Error("--output is required.");
        }

        return ValidationResult.Success();
    }
}