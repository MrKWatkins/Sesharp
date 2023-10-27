namespace MrKWatkins.DocGen.Markdown;

public sealed partial class MarkdownWriter
{
    private sealed class ParagraphWriter : IParagraphWriter
    {
        private readonly MarkdownWriter writer;

        internal ParagraphWriter(MarkdownWriter writer)
        {
            this.writer = writer;
            writer.WriteLine();
            writer.inChildBlock = true;
        }

        public void Dispose()
        {
            writer.WriteLine();
            writer.inChildBlock = false;
        }

        public void Write(string text) => writer.Write(text, true);

        public void WriteLine() => writer.WriteLine();

        public void WriteLine(string text) => writer.WriteLine(text, true);

        public void WriteCode(string code)
        {
            writer.Write("`", false);
            writer.Write(code, false);
            writer.Write("`", false);
        }

        public void WriteLink(string text, string url)
        {
            writer.Write("[", false);
            writer.Write(text, true);
            writer.Write("](", false);
            writer.Write(url, false);
            writer.Write(")", false);
        }

        public void WriteCode(string code, string url)
        {
            writer.Write("[`", false);
            writer.Write(code, false);
            writer.Write("`](", false);
            writer.Write(url, false);
            writer.Write(")", false);
        }
    }
}