using System.Web;

namespace MrKWatkins.DocGen.Markdown;

public sealed partial class MarkdownWriter : IDisposable
{
    private readonly StreamWriter writer;
    private bool inChildBlock;

    public MarkdownWriter(string path)
    {
        writer = File.CreateText(path);
    }

    private void Write(string text, bool escape) => writer.Write(escape ? HttpUtility.HtmlEncode(text) : text);

    private void WriteLine(string text, bool escape)
    {
        Write(text, escape);
        WriteLine();
    }

    private void WriteLine() => writer.WriteLine();

    public void WriteMainHeading(string text) => WriteHeading(text, 1);

    public void WriteSubHeading(string text) => WriteHeading(text, 2);

    public void WriteHeading(string text, int level)
    {
        ValidateNotInChildState();

        for (var f = 0; f < level; f++)
        {
            writer.Write('#');
        }
        writer.Write(' ');
        WriteLine(text, true);
    }

    [MustUseReturnValue]
    public ITextWriter CodeBlock()
    {
        ValidateNotInChildState();

        return new CodeWriter(this);
    }

    [MustUseReturnValue]
    public IParagraphWriter Paragraph()
    {
        ValidateNotInChildState();

        return new ParagraphWriter(this);
    }

    public void Dispose() => writer.Dispose();

    private void ValidateNotInChildState()
    {
        if (inChildBlock)
        {
            throw new InvalidOperationException("Cannot use writer when writing child block.");
        }
    }
}