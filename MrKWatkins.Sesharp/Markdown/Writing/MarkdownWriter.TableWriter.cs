namespace MrKWatkins.Sesharp.Markdown.Writing;

public sealed partial class MarkdownWriter
{
    private sealed class TableWriter : ParagraphWriter, ITableWriter
    {
        private bool inRow;

        internal TableWriter(MarkdownWriter writer, IReadOnlyList<string> headers)
            : base(writer)
        {
            if (!headers.Any())
            {
                return;
            }

            Writer.Write("|", false);
            foreach (var header in headers)
            {
                Writer.Write(" ", false);
                Writer.Write(header, true);
                Writer.Write(" |", false);
            }

            Writer.WriteLine();

            Writer.Write("|", false);
            foreach (var header in headers)
            {
                Writer.Write(" ", false);
                Writer.Write(new string('-', header.Length), true);
                Writer.Write(" |", false);
            }

            Writer.WriteLine();
        }

        public void NewColumn() => Writer.Write(" | ", false);

        public override void WriteCode(string code) => base.WriteCode(code.Replace("|", @"\|"));

        public override void WriteCode(string code, string url) => base.WriteCode(code.Replace("|", @"\|"), url);

        public void NewRow()
        {
            if (inRow)
            {
                Writer.WriteLine(" |", false);
            }

            Writer.Write("| ", false);
            inRow = true;
        }

        public override void Dispose()
        {
            if (inRow)
            {
                Writer.WriteLine(" |", false);
            }
            base.Dispose();
        }
    }
}