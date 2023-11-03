using System.Xml.Linq;

namespace MrKWatkins.DocGen.XmlDocumentation;

public sealed class MemberDocumentation
{
    private MemberDocumentation(string name, DocumentationSection? summary, IReadOnlyDictionary<string, DocumentationSection> typeParameters)
    {
        Name = name;
        Summary = summary;
        TypeParameters = typeParameters;
    }

    public string Name { get; }

    public DocumentationSection? Summary { get; }

    public IReadOnlyDictionary<string, DocumentationSection> TypeParameters { get; }

    [Pure]
    public static MemberDocumentation Parse(XElement memberXml)
    {
        var name = memberXml.Attribute("name")?.Value ?? throw new FormatException("<member> element does not have name attribute.");
        var summary = DocumentationSection.ParseOrNull(memberXml.Element("summary"));
        var typeParameters = memberXml
            .Elements("typeparam")
            .ToDictionary(
                tp => tp.Attribute("name")?.Value ?? throw new FormatException("<typeparam> element does not have name attribute."),
                DocumentationSection.Parse);

        return new MemberDocumentation(name, summary, typeParameters);
    }
}