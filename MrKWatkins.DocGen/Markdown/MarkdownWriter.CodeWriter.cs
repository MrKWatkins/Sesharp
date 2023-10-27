namespace MrKWatkins.DocGen.Markdown;

public sealed partial class MarkdownWriter
{
    private sealed class CodeWriter : ITextWriter
    {
        private readonly MarkdownWriter writer;

        internal CodeWriter(MarkdownWriter writer)
        {
            this.writer = writer;
            writer.WriteLine();
            writer.WriteLine("```c#", false);
            writer.inChildBlock = true;
        }

        public void Dispose()
        {
            writer.WriteLine();
            writer.WriteLine("```", false);
            writer.WriteLine();
            writer.inChildBlock = false;
        }

        public void Write(string text) => writer.Write(text, false);

        public void WriteLine() => writer.WriteLine();

        public void WriteLine(string text) => writer.WriteLine(text, false);
    }
}