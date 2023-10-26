using System.Xml.Linq;
using System.Xml.XPath;

namespace MrKWatkins.DocGen.XmlDocumentation;

public sealed class Documentation
{
    private readonly IReadOnlyDictionary<string, MemberDocumentation> memberDocumentation;

    private Documentation(IReadOnlyDictionary<string, MemberDocumentation> memberDocumentation)
    {
        this.memberDocumentation = memberDocumentation;
    }

    [Pure]
    public bool TryGetMemberDocumentation(string key, [MaybeNullWhen(false)] out MemberDocumentation value) => memberDocumentation.TryGetValue(key, out value);

    [Pure]
    public static Documentation Load(string path)
    {
        using var file = File.OpenRead(path);
        var xDocument = XDocument.Load(file);

        return Parse(xDocument);
    }

    [Pure]
    private static Documentation Parse(XDocument xml)
    {
        var members = new Dictionary<string, MemberDocumentation>();

        foreach (var memberXml in xml.XPathSelectElements("/doc/members/member"))
        {
            var member = MemberDocumentation.Parse(memberXml);
            members.Add(member.Name, member);
        }

        return new Documentation(members);
    }
}