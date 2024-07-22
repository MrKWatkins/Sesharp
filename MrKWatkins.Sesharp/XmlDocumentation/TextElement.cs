namespace MrKWatkins.Sesharp.XmlDocumentation;

public sealed class TextElement(string text) : DocumentationElement
{
    public string Text { get; } = text;
}