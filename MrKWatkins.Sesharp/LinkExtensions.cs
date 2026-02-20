using System.Reflection;
using MrKWatkins.Sesharp.Markdown;
using MrKWatkins.Sesharp.Markdown.Writing;
using MrKWatkins.Reflection;

namespace MrKWatkins.Sesharp;

public static class LinkExtensions
{
    [Pure]
    public static string DocumentationFileName(this MemberInfo memberInfo) => $"{memberInfo.BuildBaseFilename()}.md";

    [Pure]
    public static string DocumentationLink(this MemberInfo memberInfo, MarkdownIdFormat idFormat = MarkdownIdFormat.MkDocs) =>
        BuildMemberLink($"{memberInfo.DocumentationFileName()}", memberInfo, idFormat);

    [Pure]
    public static string MicrosoftLink(this MemberInfo memberInfo, string baseUrl = "https://learn.microsoft.com/en-gb/dotnet/api/", MarkdownIdFormat idFormat = MarkdownIdFormat.MkDocs) =>
        BuildMemberLink($"{baseUrl}{memberInfo.BuildBaseFilename()}", memberInfo, idFormat);

    [Pure]
    private static string BuildMemberLink(string baseLink, MemberInfo memberInfo, MarkdownIdFormat idFormat)
    {
        if (memberInfo is MethodBase methodBase && methodBase.HasPublicOrProtectedOverloads())
        {
            var id = MarkdownId.FromMember(methodBase);
            var fragment = idFormat == MarkdownIdFormat.Writerside ? id.Id : id.MkDocsId;
            return $"{baseLink}#{fragment}";
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
            return $"{type.Namespace}.{type.Name.Replace('`', '-')}";
        }

        type = memberInfo.DeclaringType!;

        if (type.IsEnum && memberInfo is FieldInfo)
        {
            return $"{type.Namespace}.{type.Name}".Replace('`', '-');
        }

        var memberName = memberInfo is ConstructorInfo ? "-ctor" : memberInfo.Name;

        return $"{type.Namespace}.{type.Name}.{memberName}".Replace('`', '-');
    }
}