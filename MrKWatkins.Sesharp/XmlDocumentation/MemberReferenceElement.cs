namespace MrKWatkins.Sesharp.XmlDocumentation;

public abstract class MemberReferenceElement(XmlDocId id, string? text) : DocumentationElement
{
    public XmlDocId Id { get; } = id;

    public string? Text { get; } = string.IsNullOrWhiteSpace(text) ? null : text;
}