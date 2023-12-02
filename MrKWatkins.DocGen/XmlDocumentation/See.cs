namespace MrKWatkins.DocGen.XmlDocumentation;

public sealed class See : MemberReferenceElement
{
    public See(XmlDocId id, string? text)
        : base(id, text)
    {
    }
}