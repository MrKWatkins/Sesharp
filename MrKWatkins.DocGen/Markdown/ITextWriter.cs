namespace MrKWatkins.DocGen.Markdown;

public interface ITextWriter : IDisposable
{
    void Write(string text);

    void WriteLine();

    void WriteLine(string text);
}