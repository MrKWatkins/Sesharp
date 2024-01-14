using System.Collections.Frozen;
using System.Reflection;

namespace MrKWatkins.DocGen;

public sealed class MemberLookup
{
    private readonly Assembly documentAssembly;
    private readonly IReadOnlyDictionary<XmlDocId, MemberInfo> lookup;

    public MemberLookup(Assembly documentAssembly)
    {
        this.documentAssembly = documentAssembly;

        var types = GetAllAssemblies(documentAssembly)
            .Distinct()
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
        if (rootType.Assembly == documentAssembly)
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
        var type = member is Type t ? t : member.DeclaringType!;
        while (type.DeclaringType != null)
        {
            type = type.DeclaringType;
        }

        return type;
    }

    [Pure]
    private static IEnumerable<Assembly> GetAllAssemblies(Assembly assembly) => GetAllAssemblies(new HashSet<Assembly>(), assembly);

    [Pure]
    private static IEnumerable<Assembly> GetAllAssemblies(HashSet<Assembly> processed, Assembly assembly)
    {
        if (processed.Contains(assembly))
        {
            yield break;
        }

        yield return assembly;

        processed.Add(assembly);

        foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
        {
            var referencedAssembly = Assembly.Load(referencedAssemblyName);

            foreach (var child in GetAllAssemblies(processed, referencedAssembly))
            {
                yield return child;
            }
        }
    }
}