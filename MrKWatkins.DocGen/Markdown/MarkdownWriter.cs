using System.Web;

namespace MrKWatkins.DocGen.Markdown;

public sealed class MarkdownWriter : IDisposable
{
    private readonly StreamWriter writer;
    private bool inParagraph;

    public MarkdownWriter(string path)
    {
        writer = File.CreateText(path);
    }

    public void WriteMainHeading(string text) => WriteHeading(text, 1);

    public void WriteSubHeading(string text) => WriteHeading(text, 2);

    public void WriteHeading(string text, int level)
    {
        NewParagraph();
        for (var f = 0; f < level; f++)
        {
            writer.Write('#');
        }
        writer.Write(' ');
        WriteText(text);
        writer.WriteLine();
        writer.WriteLine();
    }

    public void NewParagraph()
    {
        if (inParagraph)
        {
            writer.WriteLine();
            writer.WriteLine();
            inParagraph = false;
        }
    }

    public void WriteText(string text) => writer.Write(HttpUtility.HtmlEncode(text));

    public void Dispose()
    {
        writer.Dispose();
    }
}