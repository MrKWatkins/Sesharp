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
    public static Documentation Load(string path)
    {
        using var file = File.OpenRead(path);
        var xDocument = XDocument.Load(file, LoadOptions.PreserveWhitespace);

        return Parse(xDocument);
    }

    [Pure]
    private static Documentation Parse(XDocument xml)
    {
        var members = new Dictionary<XmlDocId, MemberDocumentation>();

        foreach (var memberXml in xml.XPathSelectElements("/doc/members/member"))
        {
            var member = MemberDocumentation.Parse(memberXml);
            members.Add(XmlDocId.Parse(member.Name), member);
        }

        return new Documentation(members);
    }
}