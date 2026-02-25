using System.Xml.Linq;

namespace MrKWatkins.Sesharp.XmlDocumentation;

public sealed class MemberDocumentation
{
    private MemberDocumentation(
        string name,
        bool hasInheritDoc,
        XmlDocId? inheritDocCref,
        DocumentationSection? summary,
        DocumentationSection? remarks,
        IReadOnlyDictionary<string, DocumentationSection> typeParameters,
        IReadOnlyDictionary<string, DocumentationSection> parameters,
        DocumentationSection? returns,
        IReadOnlyList<SeeAlso> seeAlsos)
    {
        Name = name;
        HasInheritDoc = hasInheritDoc;
        InheritDocCref = inheritDocCref;
        Summary = summary;
        Remarks = remarks;
        TypeParameters = typeParameters;
        Parameters = parameters;
        Returns = returns;
        SeeAlsos = seeAlsos;
    }

    public string Name { get; }

    public bool HasInheritDoc { get; }

    public XmlDocId? InheritDocCref { get; }

    public DocumentationSection? Summary { get; }

    public DocumentationSection? Remarks { get; }

    public IReadOnlyDictionary<string, DocumentationSection> TypeParameters { get; }

    public IReadOnlyDictionary<string, DocumentationSection> Parameters { get; }

    public DocumentationSection? Returns { get; }

    public IReadOnlyList<SeeAlso> SeeAlsos { get; }

    [Pure]
    internal MemberDocumentation MergeWithInherited(MemberDocumentation inherited) =>
        new(
            Name,
            false,
            null,
            Summary ?? inherited.Summary,
            Remarks ?? inherited.Remarks,
            MergeDict(TypeParameters, inherited.TypeParameters),
            MergeDict(Parameters, inherited.Parameters),
            Returns ?? inherited.Returns,
            SeeAlsos.Count > 0 ? SeeAlsos : inherited.SeeAlsos);

    [Pure]
    private static IReadOnlyDictionary<string, DocumentationSection> MergeDict(
        IReadOnlyDictionary<string, DocumentationSection> local,
        IReadOnlyDictionary<string, DocumentationSection> inherited)
    {
        if (local.Count == 0) return inherited;
        if (inherited.Count == 0) return local;
        var merged = new Dictionary<string, DocumentationSection>(inherited);
        foreach (var (key, value) in local)
        {
            merged[key] = value;
        }
        return merged;
    }

    [Pure]
    public static MemberDocumentation Parse(XElement memberXml)
    {
        var name = memberXml.Attribute("name")?.Value ?? throw new FormatException("<member> element does not have name attribute.");

        var inheritDocElement = memberXml.Element("inheritdoc");
        var hasInheritDoc = inheritDocElement != null;
        XmlDocId? inheritDocCref = null;
        if (inheritDocElement != null)
        {
            var cref = inheritDocElement.Attribute("cref")?.Value;
            if (cref != null)
            {
                inheritDocCref = XmlDocId.Parse(cref);
            }
        }

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

        return new MemberDocumentation(name, hasInheritDoc, inheritDocCref, summary, remarks, typeParameters, parameters, returns, seeAlso);
    }
}