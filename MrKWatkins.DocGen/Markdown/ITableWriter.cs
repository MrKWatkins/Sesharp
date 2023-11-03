namespace MrKWatkins.DocGen.Markdown;

public interface ITableWriter : IParagraphWriter
{
    void NewColumn();

    void NewRow();
}