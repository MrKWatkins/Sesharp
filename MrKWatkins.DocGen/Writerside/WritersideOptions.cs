namespace MrKWatkins.DocGen.Writerside;

public sealed class WritersideOptions
{
    public WritersideOptions(string writersideProjectDirectory)
    {
        WritersideProjectDirectory = writersideProjectDirectory;
    }

    public string WritersideProjectDirectory { get; init; }

    public string OutputDirectory { get; init; } = Path.Combine("topics", "API");

    public string OutputDirectoryPath => Path.Combine(WritersideProjectDirectory, OutputDirectory);

    public string TreeFile { get; init; } = "hi.tree";

    public string TreeFilePath => Path.Combine(WritersideProjectDirectory, TreeFile);

    public string TocElementId { get; init; } = "API";

    public string TocElementTitle { get; init; } = "API";

    public bool DeleteContentsOfOutputDirectory { get; init; } = true;

    public bool DeleteChildElements { get; init; } = true;
}