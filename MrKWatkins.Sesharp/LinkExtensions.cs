using System.Reflection;
using MrKWatkins.Sesharp.Markdown.Writing;
using MrKWatkins.Reflection;

namespace MrKWatkins.Sesharp;

public static class LinkExtensions
{
    [Pure]
    public static string DocumentationFileName(this MemberInfo memberInfo) => $"{memberInfo.BuildBaseFilename()}.md";

    [Pure]
    public static string DocumentationLink(this MemberInfo memberInfo, string? sourceFile = null)
    {
        var link = BuildMemberLink(memberInfo.DocumentationFileName(), memberInfo);
        if (sourceFile == null)
        {
            return link;
        }

        // Split off any fragment before computing the relative path.
        var hashIndex = link.IndexOf('#');
        var targetFile = hashIndex >= 0 ? link[..hashIndex] : link;
        var fragment = hashIndex >= 0 ? link[hashIndex..] : string.Empty;

        // Same file: just use the fragment (or empty if no fragment).
        if (string.Equals(targetFile, sourceFile, StringComparison.OrdinalIgnoreCase))
        {
            return fragment;
        }

        var sourceDirectory = Path.GetDirectoryName(sourceFile)!;
        var relativePath = Path.GetRelativePath(sourceDirectory, targetFile).Replace('\\', '/');
        return $"{relativePath}{fragment}";
    }

    [Pure]
    public static string MicrosoftLink(this MemberInfo memberInfo, string baseUrl = "https://learn.microsoft.com/en-gb/dotnet/api/") =>
        BuildMemberLink($"{baseUrl}{memberInfo.BuildMicrosoftFilename()}", memberInfo);

    [Pure]
    private static string BuildMemberLink(string baseLink, MemberInfo memberInfo)
    {
        if (memberInfo is MethodBase methodBase && methodBase.HasPublicOrProtectedOverloads())
        {
            var id = MarkdownId.FromMember(methodBase);
            return $"{baseLink}#{id.MkDocsId}";
        }

        if (memberInfo is FieldInfo fieldInfo && fieldInfo.DeclaringType!.IsEnum)
        {
            return $"{baseLink}#fields";
        }

        return baseLink;
    }

    [Pure]
    public static string BuildBaseFilename(this MemberInfo memberInfo)
    {
        if (memberInfo is Type type)
        {
            return $"{type.BuildTypeDirectory()}/index";
        }

        var declaringType = memberInfo.DeclaringType!;

        if (declaringType.IsEnum && memberInfo is FieldInfo)
        {
            return $"{declaringType.BuildTypeDirectory()}/index";
        }

        var memberName = memberInfo is ConstructorInfo ? "-ctor" : memberInfo.Name;

        return $"{declaringType.BuildTypeDirectory()}/{memberName}";
    }

    [Pure]
    public static string BuildTypeDirectory(this Type type)
    {
        if (!type.IsGenericType)
        {
            return $"{type.Namespace}/{type.Name}";
        }

        // Always use the generic type definition so that constructed types
        // (e.g. IntegerAssertions<Byte>) resolve to the same directory as
        // the definition (IntegerAssertions<T>).
        var definition = type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition();
        var baseName = definition.Name[..definition.Name.IndexOf('`')];
        var typeParams = string.Join("-", definition.GetGenericArguments().Select(t => t.Name));
        return $"{definition.Namespace}/{baseName}-{typeParams}";
    }

    // Used only for Microsoft docs links which use the old flat format.
    [Pure]
    private static string BuildMicrosoftFilename(this MemberInfo memberInfo)
    {
        if (memberInfo is Type type)
        {
            return $"{type.Namespace}.{type.Name.Replace('`', '-')}";
        }

        var type2 = memberInfo.DeclaringType!;

        if (type2.IsEnum && memberInfo is FieldInfo)
        {
            return $"{type2.Namespace}.{type2.Name}".Replace('`', '-');
        }

        var memberName = memberInfo is ConstructorInfo ? "-ctor" : memberInfo.Name;

        return $"{type2.Namespace}.{type2.Name}.{memberName}".Replace('`', '-');
    }
}