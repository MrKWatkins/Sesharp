namespace MrKWatkins.DocGen.Markdown.Writing;

public interface ITableWriter : IParagraphWriter
{
    void NewColumn();

    void NewRow();
}