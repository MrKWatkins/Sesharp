namespace MrKWatkins.Sesharp.XmlDocumentation;

public abstract class ReferenceElement : DocumentationElement
{
    protected ReferenceElement(string name, string? text)
    {
        Name = name;
        Text = string.IsNullOrWhiteSpace(text) ? null : text;
    }

    public string Name { get; }

    public string? Text { get; }
}