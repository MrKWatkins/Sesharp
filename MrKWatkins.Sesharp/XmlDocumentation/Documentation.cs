using System.Xml.Linq;
using System.Xml.XPath;

namespace MrKWatkins.Sesharp.XmlDocumentation;

public sealed class Documentation
{
    private readonly IReadOnlyDictionary<XmlDocId, MemberDocumentation> memberDocumentation;

    private Documentation(IReadOnlyDictionary<XmlDocId, MemberDocumentation> memberDocumentation)
    {
        this.memberDocumentation = memberDocumentation;
    }

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
    private static Documentation Parse(XDocument xml)
    {
        var members = xml.XPathSelectElements("/doc/members/member")
            .Select(MemberDocumentation.Parse)
            .ToDictionary(member => XmlDocId.Parse(member.Name));

        return new Documentation(members);
    }
}