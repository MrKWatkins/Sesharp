namespace MrKWatkins.Sesharp.Markdown.Writing;

public interface ITextWriter : IDisposable
{
    void Write(string text);

    void WriteLine();

    void WriteLine(string text);
}