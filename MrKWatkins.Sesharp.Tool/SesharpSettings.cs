using System.ComponentModel;
using MrKWatkins.Sesharp.Markdown;
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

    [CommandOption("--dialect")]
    [DefaultValue(MarkdownDialect.VitePress)]
    public MarkdownDialect Dialect { get; init; } = MarkdownDialect.VitePress;

    [CommandOption("--source-ref")]
    [DefaultValue("main")]
    public string SourceRef { get; init; } = "main";

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(OutputDirectory))
        {
            return ValidationResult.Error("--output is required.");
        }

        if (!Enum.IsDefined(Dialect))
        {
            return ValidationResult.Error("--dialect must be VitePress or MkDocs.");
        }
        if (string.IsNullOrWhiteSpace(SourceRef))
        {
            return ValidationResult.Error("--source-ref must not be empty.");
        }

        return ValidationResult.Success();
    }
}