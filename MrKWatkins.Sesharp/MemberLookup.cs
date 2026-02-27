using System.Collections.Frozen;
using System.Reflection;

namespace MrKWatkins.Sesharp;

public sealed class MemberLookup
{
    private readonly IReadOnlySet<Assembly> documentAssemblies;
    private readonly IReadOnlyDictionary<XmlDocId, MemberInfo> lookup;

    public MemberLookup(IEnumerable<Assembly> documentAssemblies)
    {
        this.documentAssemblies = documentAssemblies.ToFrozenSet();

        var types = AssemblyDependencyLoader.LoadWithDependencies(this.documentAssemblies)
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t.Namespace != null)
            .ToList();

        var members = types
            // TODO: Skip non public in referenced assemblies.
            // TODO: Implicit interface members.
            .SelectMany(t => t.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));

        // Vector2.TransformNormal is weird and has two overloads, but the types are the same. Hence the Distinct.
        lookup = members.Concat(types)
            .Select(m => new KeyValuePair<XmlDocId, MemberInfo>(XmlDocId.Create(m), m))
            .ToFrozenDictionary();
    }

    public MemberLookup(Assembly documentAssembly)
        : this([documentAssembly])
    {
    }

    [Pure]
    public (MemberInfo Member, MemberLocation Location) Get(XmlDocId id)
    {
        if (id.Id.StartsWith('N'))
        {
            throw new ArgumentException("Namespaces are not supported.", nameof(id));
        }

        if (!lookup.TryGetValue(id, out var member))
        {
            throw new KeyNotFoundException($"Could not find the type {id}.");
        }

        return (member, GetLocation(member));
    }

    [Pure]
    public MemberLocation GetLocation(MemberInfo member)
    {
        var rootType = GetRootType(member);
        if (documentAssemblies.Contains(rootType.Assembly))
        {
            return MemberLocation.DocumentAssembly;
        }

        if (rootType.Namespace?.StartsWith("System", StringComparison.Ordinal) == true)
        {
            return MemberLocation.System;
        }

        return MemberLocation.Other;
    }

    [Pure]
    private static Type GetRootType(MemberInfo member)
    {
        var type = member as Type ?? member.DeclaringType!;
        while (type.DeclaringType != null)
        {
            type = type.DeclaringType;
        }

        return type;
    }
}