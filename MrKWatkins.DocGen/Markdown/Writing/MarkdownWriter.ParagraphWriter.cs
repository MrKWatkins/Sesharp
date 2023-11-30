namespace MrKWatkins.DocGen.Markdown.Writing;

public sealed partial class MarkdownWriter
{
    private class ParagraphWriter : IParagraphWriter
    {
        internal ParagraphWriter(MarkdownWriter writer)
        {
            Writer = writer;
            writer.WriteLine();
            writer.inChildBlock = true;
        }

        protected MarkdownWriter Writer { get; }

        public virtual void Dispose()
        {
            Writer.WriteLine();
            Writer.inChildBlock = false;
        }

        public void Write(string text) => Writer.Write(text, true);

        public void WriteLine() => Writer.WriteLine();

        public void WriteLine(string text) => Writer.WriteLine(text, true);

        public void WriteCode(string code)
        {
            Writer.Write("`", false);
            Writer.Write(code, false);
            Writer.Write("`", false);
        }

        public void WriteLink(string text, string url)
        {
            Writer.Write("[", false);
            Writer.Write(text, true);
            Writer.Write("](", false);
            Writer.Write(url, false);
            Writer.Write(")", false);
        }

        public void WriteCode(string code, string url)
        {
            Writer.Write("[`", false);
            Writer.Write(code, false);
            Writer.Write("`](", false);
            Writer.Write(url, false);
            Writer.Write(")", false);
        }
    }
}