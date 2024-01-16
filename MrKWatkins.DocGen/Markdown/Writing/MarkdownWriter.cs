using System.Web;

namespace MrKWatkins.DocGen.Markdown.Writing;

public sealed partial class MarkdownWriter : IDisposable
{
    private readonly StreamWriter writer;
    private bool inChildBlock;
    private MarkdownId? idSuffix;

    public MarkdownWriter(string path)
    {
        writer = File.CreateText(path);
    }

    private void Write(string text, bool escape) => writer.Write(escape ? Escape(text) : text);

    private void WriteLine(string text, bool escape)
    {
        Write(text, escape);
        WriteLine();
    }

    [Pure]
    private static string Escape(string text) => HttpUtility.HtmlEncode(text).Replace("[]", @"\[\]");

    private void WriteLine() => writer.WriteLine();

    [MustUseReturnValue]
    public IDisposable WithIdSuffix(MarkdownId suffix)
    {
        if (idSuffix != null)
        {
            throw new InvalidOperationException("Suffix already set.");
        }

        idSuffix = suffix;

        return new Disposable(() => idSuffix = null);
    }

    public void WriteMainHeading(string text, MarkdownId? id = null) => WriteHeading(text, 1, id);

    public void WriteSubHeading(string text, MarkdownId? id = null) => WriteHeading(text, 2, id);

    public void WriteSubSubHeading(string text, MarkdownId? id = null) => WriteHeading(text, 3, id);

    public void WriteHeading(string text, int level, MarkdownId? id)
    {
        ValidateNotInChildState();

        for (var f = 0; f < level; f++)
        {
            writer.Write('#');
        }
        writer.Write(' ');
        Write(text, true);

        if (idSuffix != null)
        {
            id = (id ?? MarkdownId.FromText(text)) + idSuffix;
        }

        if (id != null)
        {
            Write($" {{id=\"{id}\"}}", false);
        }

        WriteLine();
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

    [MustUseReturnValue]
    public ITableWriter Table(params string[] headers)
    {
        ValidateNotInChildState();

        return new TableWriter(this, headers);
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