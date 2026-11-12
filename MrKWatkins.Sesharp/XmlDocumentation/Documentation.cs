using System.Xml.Linq;
using System.Xml.XPath;

namespace MrKWatkins.Sesharp.XmlDocumentation;

public sealed class Documentation
{
    private readonly IReadOnlyDictionary<XmlDocId, MemberDocumentation> memberDocumentation;

    private Documentation(IReadOnlyDictionary<XmlDocId, MemberDocumentation> memberDocumentation) => this.memberDocumentation = memberDocumentation;

    [Pure]
    public MemberDocumentation? GetMemberDocumentationOrNull(XmlDocId key) => memberDocumentation.GetValueOrDefault(key);

    [Pure]
    public static Documentation Load(IFileSystem fileSystem, string path)
    {
        using var file = fileSystem.OpenRead(path);
        var xDocument = XDocument.Load(file, LoadOptions.PreserveWhitespace);

        return Parse(xDocument);
    }

    [Pure]
    internal static Documentation Load(string path)
    {
        using var file = File.OpenRead(path);
        var xDocument = XDocument.Load(file, LoadOptions.PreserveWhitespace);

        return Parse(xDocument);
    }

    [Pure]
    private static Documentation Parse(XDocument xml)
    {
        var elements = xml.XPathSelectElements("/doc/members/member")
            .ToDictionary(
                member => member.Attribute("name")?.Value ?? throw new FormatException("<member> element does not have name attribute."),
                StringComparer.Ordinal);

        foreach (var (name, member) in elements)
        {
            if (!name.StartsWith("M:", StringComparison.Ordinal))
            {
                continue;
            }

            var constructorIndex = name.IndexOf(".#ctor", StringComparison.Ordinal);
            if (constructorIndex < 0)
            {
                continue;
            }

            var typeName = "T:" + name[2..constructorIndex];
            if (!elements.TryGetValue(typeName, out var type) || !member.Elements().SequenceEqual(type.Elements(), XNode.EqualityComparer))
            {
                continue;
            }

            // Primary constructor XML duplicates the type documentation.
            var summary = new XElement(
                "summary",
                "Initializes a new instance of the ",
                new XElement("see", new XAttribute("cref", typeName)),
                " type.");
            if (member.Element("summary") is { } existing)
            {
                existing.ReplaceWith(summary);
            }
            else
            {
                member.AddFirst(summary);
            }
        }

        var members = elements.Values.Select(MemberDocumentation.Parse).ToDictionary(member => XmlDocId.Parse(member.Name));

        return new Documentation(members);
    }
}