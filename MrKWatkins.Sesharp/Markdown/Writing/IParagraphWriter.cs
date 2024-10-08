namespace MrKWatkins.Sesharp.Markdown.Writing;

public interface IParagraphWriter : ITextWriter
{
    void WriteCode(string code);

    void WriteLink(string text, string url);

    void WriteCode(string code, string url);
}