using System.Reflection;

namespace MrKWatkins.Sesharp.XmlDocumentation;

internal sealed class InheritDocResolver
{
    private readonly Dictionary<Assembly, Documentation?> documentationByAssembly;

    // Caches Documentation objects loaded from individual reference pack XML files.
    // Keyed by file path so each file is only parsed once even when multiple assemblies
    // point to the same reference pack directory.
    private readonly Dictionary<string, Documentation?> refPackDocumentation = new();

    public InheritDocResolver(Assembly documentAssembly, Documentation documentation)
    {
        documentationByAssembly = new Dictionary<Assembly, Documentation?> { [documentAssembly] = documentation };
    }

    [Pure]
    public MemberDocumentation? Resolve(MemberInfo member) =>
        ResolveForMember(member, XmlDocId.Create(member), []);

    [Pure]
    private MemberDocumentation? ResolveForMember(MemberInfo member, XmlDocId id, HashSet<XmlDocId> visited)
    {
        if (!visited.Add(id))
            return null;

        var memberDoc = FindMemberDocumentation(GetAssembly(member), id);
        if (memberDoc == null || !memberDoc.HasInheritDoc)
            return memberDoc;

        if (memberDoc.InheritDocCref != null)
            return ResolveByCref(memberDoc.InheritDocCref, visited);

        var sourceMember = FindNaturalParent(member);
        if (sourceMember == null)
            return null;

        // XmlDocId.Create requires open generic types; convert if the parent is on a constructed generic.
        sourceMember = ToOpenMember(sourceMember);

        return ResolveForMember(sourceMember, XmlDocId.Create(sourceMember), visited);
    }

    [Pure]
    private MemberDocumentation? ResolveByCref(XmlDocId cref, HashSet<XmlDocId> visited)
    {
        if (!visited.Add(cref))
            return null;

        // Search documentation already loaded into the cache.
        foreach (var docs in documentationByAssembly.Values)
        {
            if (docs == null) continue;
            var doc = docs.GetMemberDocumentationOrNull(cref);
            if (doc != null)
                return doc.HasInheritDoc ? null : doc; // Don't recurse further for cref case.
        }

        // Try all currently loaded assemblies.
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (documentationByAssembly.ContainsKey(assembly)) continue;
            var docs = LoadDocumentation(assembly);
            if (docs == null) continue;
            var doc = docs.GetMemberDocumentationOrNull(cref);
            if (doc != null)
                return doc.HasInheritDoc ? null : doc;
        }

        return null;
    }

    private MemberDocumentation? FindMemberDocumentation(Assembly assembly, XmlDocId id)
    {
        // Fast path: try the XML file that directly corresponds to the assembly.
        var docs = LoadDocumentation(assembly);
        var memberDoc = docs?.GetMemberDocumentationOrNull(id);
        if (memberDoc != null)
            return memberDoc;

        // Fallback for assemblies like System.Private.CoreLib whose docs are spread across
        // many reference pack XML files rather than a single matching file.
        return FindInReferencePackXmls(assembly, id);
    }

    private MemberDocumentation? FindInReferencePackXmls(Assembly assembly, XmlDocId id)
    {
        var refPackDir = AssemblyXmlDocumentationFinder.FindReferencePackDirectory(assembly);
        if (refPackDir == null)
            return null;

        foreach (var xmlPath in Directory.GetFiles(refPackDir, "*.xml"))
        {
            if (!refPackDocumentation.TryGetValue(xmlPath, out var docs))
            {
                try
                {
                    docs = Documentation.Load(xmlPath);
                }
                catch (Exception)
                {
                    docs = null;
                }
                refPackDocumentation[xmlPath] = docs;
            }

            var memberDoc = docs?.GetMemberDocumentationOrNull(id);
            if (memberDoc != null)
                return memberDoc;
        }

        return null;
    }

    private Documentation? LoadDocumentation(Assembly assembly)
    {
        if (documentationByAssembly.TryGetValue(assembly, out var docs))
            return docs;

        var xmlPath = AssemblyXmlDocumentationFinder.FindXmlPath(assembly);
        Documentation? result = null;
        if (xmlPath != null)
        {
            try
            {
                result = Documentation.Load(xmlPath);
            }
            catch (Exception)
            {
                // Ignore errors loading external documentation.
            }
        }

        documentationByAssembly[assembly] = result;
        return result;
    }

    [Pure]
    private static Assembly GetAssembly(MemberInfo member) =>
        member is Type type ? type.Assembly : member.DeclaringType!.Assembly;

    // XmlDocId.Create throws for members on constructed generic types (e.g. INumberBase<UInt24>).
    // This converts such members to their open generic equivalents (e.g. INumberBase<T>).
    [Pure]
    private static MemberInfo ToOpenMember(MemberInfo member)
    {
        if (member is Type type)
            return type.IsConstructedGenericType ? type.GetGenericTypeDefinition() : type;

        var declaringType = member.DeclaringType;
        if (declaringType == null || !declaringType.IsConstructedGenericType)
            return member;

        return declaringType.GetGenericTypeDefinition().GetMemberWithSameMetadataDefinitionAs(member);
    }

    [Pure]
    private static MemberInfo? FindNaturalParent(MemberInfo member) =>
        member switch
        {
            MethodInfo method => FindMethodParent(method),
            PropertyInfo property => FindPropertyParent(property),
            Type type => FindTypeParent(type),
            EventInfo evt => FindEventParent(evt),
            _ => null
        };

    [Pure]
    private static MemberInfo? FindMethodParent(MethodInfo method)
    {
        // Check if the method overrides a base class method.
        var baseMethod = method.GetBaseDefinition();
        if (baseMethod != method)
            return baseMethod;

        // Search the declaring type's interfaces for a method with a matching signature.
        // We search on the constructed interface (e.g. INumberBase<UInt24>) so that parameter
        // types are resolved and can be compared directly. ToOpenMember in the caller then
        // converts the result back to the open generic (e.g. INumberBase<T>).
        // This approach also handles static abstract interface members, which GetInterfaceMap
        // does not reliably cover.
        var declaringType = method.DeclaringType!;
        var methodParams = method.GetParameters();
        foreach (var iface in declaringType.GetInterfaces())
        {
            foreach (var candidate in iface.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (candidate.Name != method.Name)
                    continue;

                var candidateParams = candidate.GetParameters();
                if (candidateParams.Length != methodParams.Length)
                    continue;

                var match = true;
                for (var i = 0; i < candidateParams.Length; i++)
                {
                    if (candidateParams[i].ParameterType != methodParams[i].ParameterType)
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return candidate;
            }
        }

        return null;
    }

    [Pure]
    private static MemberInfo? FindPropertyParent(PropertyInfo property)
    {
        var declaringType = property.DeclaringType!;

        // Use the accessor to detect a base-class override.
        var accessor = property.GetMethod ?? property.SetMethod;
        if (accessor != null)
        {
            var baseAccessor = accessor.GetBaseDefinition();
            if (baseAccessor != accessor)
            {
                var baseProperty = baseAccessor.DeclaringType!.GetProperty(
                    property.Name,
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (baseProperty != null)
                    return baseProperty;
            }
        }

        // Check interfaces.
        foreach (var iface in declaringType.GetInterfaces())
        {
            var ifaceProperty = iface.GetProperty(property.Name);
            if (ifaceProperty != null)
                return ifaceProperty;
        }

        return null;
    }

    [Pure]
    private static MemberInfo? FindTypeParent(Type type)
    {
        if (type.BaseType != null && type.BaseType != typeof(object))
            return type.BaseType;

        var interfaces = type.GetInterfaces();
        return interfaces.Length > 0 ? interfaces[0] : null;
    }

    [Pure]
    private static MemberInfo? FindEventParent(EventInfo evt)
    {
        var declaringType = evt.DeclaringType!;

        var baseType = declaringType.BaseType;
        while (baseType != null)
        {
            var baseEvent = baseType.GetEvent(evt.Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (baseEvent != null)
                return baseEvent;
            baseType = baseType.BaseType;
        }

        foreach (var iface in declaringType.GetInterfaces())
        {
            var ifaceEvent = iface.GetEvent(evt.Name);
            if (ifaceEvent != null)
                return ifaceEvent;
        }

        return null;
    }
}