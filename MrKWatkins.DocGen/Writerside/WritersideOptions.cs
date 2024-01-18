namespace MrKWatkins.DocGen.Writerside;

public sealed class WritersideOptions
{
    public WritersideOptions(string writersideProjectDirectory)
    {
        WritersideProjectDirectory = writersideProjectDirectory;
    }

    public string WritersideProjectDirectory { get; set; }

    public string OutputDirectory { get; set; } = Path.Combine("topics", "API");

    public string OutputDirectoryPath => Path.Combine(WritersideProjectDirectory, OutputDirectory);

    public string TreeFile { get; set; } = "hi.tree";

    public string TreeFilePath => Path.Combine(WritersideProjectDirectory, TreeFile);

    public string TocElementId { get; set; } = "API";

    public string TocElementTitle { get; set; } = "API";

    public bool DeleteContentsOfOutputDirectory { get; set; } = true;

    public bool DeleteChildElements { get; set; } = true;
}