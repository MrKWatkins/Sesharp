using System.Xml.Linq;

namespace MrKWatkins.Sesharp.XmlDocumentation;

public sealed class MemberDocumentation
{
    private MemberDocumentation(
        string name,
        DocumentationSection? summary,
        DocumentationSection? remarks,
        IReadOnlyDictionary<string, DocumentationSection> typeParameters,
        IReadOnlyDictionary<string, DocumentationSection> parameters,
        DocumentationSection? returns,
        IReadOnlyList<SeeAlso> seeAlsos)
    {
        Name = name;
        Summary = summary;
        Remarks = remarks;
        TypeParameters = typeParameters;
        Parameters = parameters;
        Returns = returns;
        SeeAlsos = seeAlsos;
    }

    public string Name { get; }

    public DocumentationSection? Summary { get; }

    public DocumentationSection? Remarks { get; }

    public IReadOnlyDictionary<string, DocumentationSection> TypeParameters { get; }

    public IReadOnlyDictionary<string, DocumentationSection> Parameters { get; }

    public DocumentationSection? Returns { get; }

    public IReadOnlyList<SeeAlso> SeeAlsos { get; }

    [Pure]
    public static MemberDocumentation Parse(XElement memberXml)
    {
        var name = memberXml.Attribute("name")?.Value ?? throw new FormatException("<member> element does not have name attribute.");

        var summary = DocumentationSection.ParseOrNull(memberXml.Element("summary"));
        var remarks = DocumentationSection.ParseOrNull(memberXml.Element("remarks"));

        var typeParameters = memberXml
            .Elements("typeparam")
            .ToDictionary(
                tp => tp.Attribute("name")?.Value ?? throw new FormatException("<typeparam> element does not have name attribute."),
                DocumentationSection.Parse);

        var parameters = memberXml
            .Elements("param")
            .ToDictionary(
                tp => tp.Attribute("name")?.Value ?? throw new FormatException("<param> element does not have name attribute."),
                DocumentationSection.Parse);

        var returns = DocumentationSection.ParseOrNull(memberXml.Element("returns"));

        var seeAlso = memberXml.Elements("seealso").Select(XmlDocumentation.SeeAlso.Parse).ToList();

        return new MemberDocumentation(name, summary, remarks, typeParameters, parameters, returns, seeAlso);
    }
}