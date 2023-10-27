using System.Xml.Linq;

namespace MrKWatkins.DocGen.XmlDocumentation;

public sealed class MemberDocumentation
{
    private MemberDocumentation(string name, DocumentationSection? summary)
    {
        Name = name;
        Summary = summary;
    }

    public string Name { get; }

    public DocumentationSection? Summary { get; }

    [Pure]
    public static MemberDocumentation Parse(XElement memberXml) =>
        new(
            memberXml.Attribute("name")?.Value ?? throw new FormatException("<member> element does not have name attribute."),
            DocumentationSection.Parse(memberXml.Element("summary")));
}