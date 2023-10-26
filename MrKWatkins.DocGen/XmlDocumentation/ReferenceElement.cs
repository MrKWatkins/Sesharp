namespace MrKWatkins.DocGen.XmlDocumentation;

public abstract class ReferenceElement : DocumentationElement
{
    protected ReferenceElement(string key, string? text)
    {
        Key = key;
        Text = string.IsNullOrWhiteSpace(text) ? key : text;
    }

    public string Key { get; }

    public string Text { get; }
}