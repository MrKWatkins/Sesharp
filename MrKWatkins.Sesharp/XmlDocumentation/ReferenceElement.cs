namespace MrKWatkins.Sesharp.XmlDocumentation;

public abstract class ReferenceElement(string name, string? text) : DocumentationElement
{
    public string Name { get; } = name;

    public string? Text { get; } = string.IsNullOrWhiteSpace(text) ? null : text;
}