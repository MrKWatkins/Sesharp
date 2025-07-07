using System.Xml.Linq;

namespace MrKWatkins.Sesharp.XmlDocumentation;

public sealed class SeeAlso(string? text, XmlDocId? cref, string? href) : DocumentationElement
{
    public string? Text { get; } = text;

    public XmlDocId? Cref { get; } = cref;

    public string? Href { get; } = href;

    [Pure]
    internal static SeeAlso Parse(XElement seeAlso)
    {
        var cref = seeAlso.Attribute("cref")?.Value;
        return new SeeAlso(
            seeAlso.Value,
            cref != null ? XmlDocId.Parse(cref) : null,
            seeAlso.Attribute("href")?.Value);
    }
}