using System.Reflection;
using MrKWatkins.Reflection.Formatting;

namespace MrKWatkins.Sesharp;

// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/#id-strings
public sealed class XmlDocId : IEquatable<XmlDocId>
{
    private static readonly IReflectionFormatter Formatter = new CachedReflectionFormatter(new XmlDocIdFormatter());

    private XmlDocId(string id)
    {
        Id = id;
    }

    public string Id { get; }

    public override string ToString() => Id;

    // TODO: Some validation.
    [Pure]
    public static XmlDocId Parse(string id) => new(id);

    [Pure]
    public static XmlDocId Create(MemberInfo member) => new(Formatter.Format(member));

    public bool Equals(XmlDocId? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Id.Equals(other.Id, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) => Equals(obj as XmlDocId);

    public override int GetHashCode() => Id.GetHashCode(StringComparison.OrdinalIgnoreCase);

    public static bool operator ==(XmlDocId? left, XmlDocId? right) => Equals(left, right);

    public static bool operator !=(XmlDocId? left, XmlDocId? right) => !Equals(left, right);
}