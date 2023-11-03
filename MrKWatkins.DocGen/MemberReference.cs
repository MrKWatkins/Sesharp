using System.Reflection;

namespace MrKWatkins.DocGen;

public sealed class MemberReference
{
    private MemberReference(string key, ReferenceType referenceType, Type type, TypeLocation location, MemberInfo? member)
    {
        Key = key;
        ReferenceType = referenceType;
        Type = type;
        Location = location;
        Member = member ?? Type;
    }

    public string Key { get; }

    public ReferenceType ReferenceType { get; }

    public Type Type { get; }

    public TypeLocation Location { get; }

    public MemberInfo Member { get; }

    [Pure]
    public static MemberReference Parse(TypeLookup typeLookup, string key)
    {
        var referenceType = ParseReferenceType(key);
        var typeName = key[2..];
        string? memberName = null;

        if (referenceType != ReferenceType.Type)
        {
            var lastSeparatorIndex = typeName.LastIndexOf('.');
            memberName = typeName[(lastSeparatorIndex + 1)..];
            typeName = typeName[..lastSeparatorIndex];
        }

        var (type, location) = typeLookup.Get(typeName);

        var member = memberName != null
            ? type.GetMember(memberName).FirstOrDefault() ?? throw new InvalidOperationException($"Member {memberName} not found on type {typeName}.")
            : null;

        return new MemberReference(key, referenceType, type, location, member);
    }

    [Pure]
    private static ReferenceType ParseReferenceType(string key) =>
        key[0] switch
        {
            'F' => ReferenceType.Field,
            'M' => ReferenceType.Method,
            'P' => ReferenceType.Property,
            'T' => ReferenceType.Type,
            _ => throw new NotSupportedException($"The key prefix {key[0]} is not supported.")
        };
}