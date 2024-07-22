namespace MrKWatkins.Sesharp.XmlDocumentation;

public sealed class TextElement : DocumentationElement
{
    public TextElement(string text)
    {
        Text = text;
    }

    public string Text { get; }
}