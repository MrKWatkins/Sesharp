using System.Reflection;

namespace MrKWatkins.DocGen;

public static class MemberInfoExtensions
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
            return type.Name.Replace('`', '-');
        }

        type = memberInfo.DeclaringType!;
        return type.Name.Replace('`', '-');
    }
}