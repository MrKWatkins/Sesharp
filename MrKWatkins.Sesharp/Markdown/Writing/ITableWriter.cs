namespace MrKWatkins.Sesharp.Markdown.Writing;

public interface ITableWriter : IParagraphWriter
{
    void NewColumn();

    void NewRow();
}