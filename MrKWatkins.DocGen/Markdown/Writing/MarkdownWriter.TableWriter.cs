namespace MrKWatkins.DocGen.Markdown.Writing;

public sealed partial class MarkdownWriter
{
    private sealed class TableWriter : ParagraphWriter, ITableWriter
    {
        private bool inRow;
        private int columnCount;

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

        public void NewColumn()
        {
            Writer.Write(" | ", false);
            columnCount++;
        }

        public void NewRow()
        {
            if (inRow)
            {
                Writer.WriteLine(" |", false);
            }

            Writer.Write("| ", false);
            inRow = true;
            columnCount = 1;
        }

        public override void Dispose()
        {
            Writer.WriteLine(" |", false);
            for (var f = 0; f < columnCount; f++)
            {
                Writer.Write("|---", false);
            }
            Writer.WriteLine("|", false);
            base.Dispose();
        }
    }
}