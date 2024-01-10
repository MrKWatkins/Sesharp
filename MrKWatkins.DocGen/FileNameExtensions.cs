using System.Reflection;

namespace MrKWatkins.DocGen;

public static class FileNameExtensions
{
    [Pure]
    public static string DocumentationFileName(this MemberInfo memberInfo) => $"{memberInfo.BaseFilename()}.md";

    [Pure]
    public static string MicrosoftFileName(this MemberInfo memberInfo, string baseUrl = "https://learn.microsoft.com/en-gb/dotnet/api/") =>
        $"{baseUrl}{memberInfo.BaseFilename()}";

    [Pure]
    private static string BaseFilename(this MemberInfo memberInfo)
    {
        if (memberInfo is Type type)
        {
            return $"{type.Namespace}.{type.Name.Replace('`', '-')}";
        }

        type = memberInfo.DeclaringType!;
        if (memberInfo is ConstructorInfo)
        {
            return $"{type.Namespace}.{type.Name.Replace('`', '-')}.-ctor";
        }

        return $"{type.Namespace}.{type.Name}.{memberInfo.Name}".Replace('`', '-');
    }
}