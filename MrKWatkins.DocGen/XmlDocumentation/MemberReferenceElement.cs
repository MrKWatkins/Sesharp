namespace MrKWatkins.DocGen.XmlDocumentation;

public abstract class MemberReferenceElement : DocumentationElement
{
    protected MemberReferenceElement(XmlDocId id, string? text)
    {
        Id = id;
        Text = string.IsNullOrWhiteSpace(text) ? null : text;
    }

    public XmlDocId Id { get; }

    public string? Text { get; }
}