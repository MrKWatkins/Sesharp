namespace MrKWatkins.DocGen.Writerside;

public interface IWritersideOptions
{
    string TreeFile { get; }

    string TocElementId { get; }

    string TocElementTitle { get; }
}