using System.Reflection;
using System.Text.RegularExpressions;
using MrKWatkins.Reflection;
using Type = System.Type;

namespace MrKWatkins.Sesharp.Markdown.Writing;

public sealed class MarkdownId
{
    private const string Separator = "-";
    private static readonly Regex ReplaceCharacters = new($"[^a-z0-9()*@{Separator}]", RegexOptions.Compiled);
    private static readonly Regex MultipleSeparators = new($"{Separator}{Separator}+", RegexOptions.Compiled);
    private static readonly Regex MkDocsReplaceCharacters = new(@"[()*@]", RegexOptions.Compiled);

    private MarkdownId(string id)
    {
        Id = id;
    }

    public string Id { get; }

    public string MkDocsId
    {
        get
        {
            var id = MkDocsReplaceCharacters.Replace(Id, "_");
            id = MultipleSeparators.Replace(id, Separator);
            return id.Trim('-');
        }
    }

    public override string ToString() => Id;

    [Pure]
    public static MarkdownId operator +(MarkdownId prefix, MarkdownId suffix) => new($"{prefix.Id}{Separator}{suffix.Id}");

    [Pure]
    public static MarkdownId FromText(string title)
    {
        var id = title.Trim().ToLowerInvariant();

        id = ReplaceCharacters.Replace(id, Separator);

        id = MultipleSeparators.Replace(id, Separator);

        return new MarkdownId(id);
    }

    [Pure]
    public static MarkdownId FromMember(MemberInfo member) =>
        member switch
        {
            ConstructorInfo constructor => FromMethod(constructor),
            MethodInfo method => FromMethod(method),
            PropertyInfo property => FromProperty(property),
            _ => throw new NotSupportedException($"Members of type {member.GetType().ToDisplayName()} are not supported.")
        };

    [Pure]
    private static MarkdownId FromMethod(MethodBase method)
    {
        var type = method.DeclaringType!;
        var id = $"{GetTypeId(type)}{Separator}{(method is ConstructorInfo ? "ctor" : method.Name)}";

        if (method.IsGenericMethod)
        {
            id = $"{id}-{method.GetGenericArguments().Length}";
        }

        var parameters = method.GetParameters();

        return FromText(parameters.Any()
            ? $"{id}({string.Join(Separator, parameters.Select(GetParameterTypeId))})"
            : id);
    }

    [Pure]
    private static MarkdownId FromProperty(PropertyInfo property)
    {
        var indexParameters = property.GetIndexParameters();
        if (indexParameters.Length == 0)
        {
            throw new NotSupportedException("Only indexer properties are supported.");
        }

        var type = property.DeclaringType!;
        var id = $"{GetTypeId(type)}{Separator}item({string.Join(Separator, indexParameters.Select(GetParameterTypeId))})";
        return FromText(id);
    }

    [Pure]
    private static string GetTypeId(Type type) => $"{type.Namespace}{Separator}{type.Name}";

    [Pure]
    private static string GetParameterTypeId(ParameterInfo parameter) => GetParameterTypeId(parameter, parameter.ParameterType);

    [Pure]
    private static string GetParameterTypeId(ParameterInfo parameter, Type type)
    {
        if (type.IsByRef)
        {
            return $"{GetParameterTypeId(parameter, type.GetElementType()!)}@";
        }

        if (type.IsArray)
        {
            return $"{GetParameterTypeId(parameter, type.GetElementType()!)}()";
        }

        if (type.IsGenericTypeParameter)
        {
            var methodType = parameter.Member.DeclaringType!;
            var index = Array.IndexOf(methodType.GetGenericArguments(), type);
            if (index == -1)
            {
                throw new InvalidOperationException("Could not find generic type parameter generic type in parent type.");
            }

            return $"{Separator}{index}";
        }

        if (type.IsGenericMethodParameter)
        {
            var index = Array.IndexOf(((MethodInfo)parameter.Member).GetGenericArguments(), type);
            if (index == -1)
            {
                throw new InvalidOperationException("Could not find generic method parameter in parent method.");
            }

            return $"{Separator}{index}";
        }

        if (type.IsGenericType)
        {
            return $"{type.Namespace}{Separator}{type.Name[..^2]}(({string.Join(Separator, type.GetGenericArguments().Select(t => GetParameterTypeId(parameter, t)))}))";
        }

        return $"{type.Namespace}{Separator}{type.Name}";
    }
}