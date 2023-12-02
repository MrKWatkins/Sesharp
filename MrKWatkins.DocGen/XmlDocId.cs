using System.Reflection;
using System.Text;
using Type = System.Type;

namespace MrKWatkins.DocGen;

// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/#id-strings
public sealed class XmlDocId : IEquatable<XmlDocId>
{
    private XmlDocId(string id)
    {
        Id = id;
    }

    public string Id { get; }

    public override string ToString() => Id;

    // TODO: Some validation.
    [Pure]
    public static XmlDocId Create(string id) => new(id);

    [Pure]
    public static XmlDocId Create(MemberInfo member) =>
        member switch
        {
            ConstructorInfo constructor => Create(constructor),
            EventInfo @event => Create(@event),
            FieldInfo field => Create(field),
            MethodInfo method => Create(method),
            PropertyInfo property => Create(property),
            Type type => Create(type),
            _ => throw new NotSupportedException($"Members of type {member.GetType().DisplayName()} are not supported.")
        };

    [Pure]
    public static XmlDocId Create(Type type)
    {
        var id = new StringBuilder("T:");
        AppendType(id, type);
        return new XmlDocId(id.ToString());
    }

    [Pure]
    public static XmlDocId Create(EventInfo @event)
    {
        var id = new StringBuilder("E:");
        AppendType(id, @event.DeclaringType!);
        id.Append('.');
        AppendName(id, @event);
        return new XmlDocId(id.ToString());
    }

    [Pure]
    public static XmlDocId Create(FieldInfo field)
    {
        var id = new StringBuilder("F:");
        AppendType(id, field.DeclaringType!);
        id.Append('.');
        AppendName(id, field);
        return new XmlDocId(id.ToString());
    }

    [Pure]
    public static XmlDocId Create(PropertyInfo property)
    {
        var id = new StringBuilder("P:");
        AppendType(id, property.DeclaringType!);
        id.Append('.');
        AppendName(id, property);
        AppendParameters(id, property.GetIndexParameters());

        return new XmlDocId(id.ToString());
    }

    [Pure]
    public static XmlDocId Create(MethodInfo method)
    {
        var id = new StringBuilder("M:");
        Append(id, method);

        if (method.Name is "op_Implicit" or "op_Explicit" or "op_CheckedExplicit")
        {
            id.Append('~');
            AppendParameterType(id, method, method.ReturnType);
        }
        return new XmlDocId(id.ToString());
    }

    [Pure]
    public static XmlDocId Create(ConstructorInfo constructor)
    {
        var id = new StringBuilder("M:");
        Append(id, constructor);
        return new XmlDocId(id.ToString());
    }

    private static void Append(StringBuilder id, MethodBase method)
    {
        AppendType(id, method.DeclaringType!);  // Ignoring global methods on a module; we don't generate documentation for them.
        id.Append('.');

        AppendName(id, method);

        if (method.IsGenericMethod)
        {
            id.Append("``");
            id.Append(method.GetGenericArguments().Length);
        }

        AppendParameters(id, method.GetParameters());
    }

    private static void AppendParameters(StringBuilder id, IReadOnlyList<ParameterInfo> parameters)
    {
        if (parameters.Count == 0)
        {
            return;
        }

        id.Append('(');
        foreach (var parameter in parameters)
        {
            if (parameter != parameters[0])
            {
                id.Append(',');
            }

            AppendParameterType(id, parameter.Member, parameter.ParameterType);
        }
        id.Append(')');
    }

    private static void AppendParameterType(StringBuilder id, MemberInfo member, Type type)
    {
        if (type.IsPointer)
        {
            AppendParameterTypePointer(id, member, type);
            return;
        }

        if (type.IsArray)
        {
            AppendParameterTypeArray(id, member, type);
            return;
        }

        if (type.IsByRef)
        {
            AppendParameterTypeByRef(id, member, type);
            return;
        }

        if (type.IsGenericMethodParameter)
        {
            AppendParameterTypeGenericMethodParameter(id, member, type);
            return;
        }

        if (type.IsGenericTypeParameter)
        {
            AppendParameterTypeGenericTypeParameter(id, member, type);
            return;
        }

        if (type.IsGenericType)
        {
            AppendParameterTypeGenericType(id, member, type);
            return;
        }

        AppendNamespace(id, type);
        AppendName(id, type);
    }

    private static void AppendParameterTypeGenericType(StringBuilder id, MemberInfo member, Type type)
    {
        AppendNamespace(id, type);
        AppendName(id, type.GetGenericTypeDefinition(), false);

        id.Append('{');
        var typeArguments = type.GetGenericArguments();
        foreach (var typeArgument in typeArguments)
        {
            if (typeArgument != typeArguments[0])
            {
                id.Append(',');
            }

            AppendParameterType(id, member, typeArgument);
        }

        id.Append('}');
    }

    private static void AppendParameterTypeGenericTypeParameter(StringBuilder id, MemberInfo member, Type type)
    {
        var typeArguments = member
            .DeclaringType!
            .GetGenericTypeDefinition()
            .GetGenericArguments();

        var index = Array.IndexOf(typeArguments, type);

        id.Append('`').Append(index);
    }

    private static void AppendParameterTypeGenericMethodParameter(StringBuilder id, MemberInfo member, Type type)
    {
        var method = (MethodBase)member;
        var methodArguments = method.GetGenericArguments();

        var index = Array.IndexOf(methodArguments, type);

        id.Append("``").Append(index);
    }

    private static void AppendParameterTypePointer(StringBuilder id, MemberInfo member, Type type)
    {
        AppendParameterType(id, member, type.GetElementType()!);
        id.Append('*');
    }

    private static void AppendParameterTypeArray(StringBuilder id, MemberInfo member, Type type)
    {
        AppendParameterType(id, member, type.GetElementType()!);
        id.Append('[');
        for (var f = 0; f < type.GetArrayRank() - 1; f++)
        {
            id.Append(',');
        }
        id.Append(']');
    }

    private static void AppendParameterTypeByRef(StringBuilder id, MemberInfo member, Type type)
    {
        AppendParameterType(id, member, type.GetElementType()!);
        id.Append('@');
    }

    private static void AppendType(StringBuilder id, Type type)
    {
        AppendNamespace(id, type);
        foreach (var nested in type.EnumerateNestedTypes())
        {
            AppendName(id, nested);
            if (nested != type)
            {
                id.Append('.');
            }
        }
    }

    private static void AppendNamespace(StringBuilder id, Type type)
    {
        if (type.Namespace != null)
        {
            id.Append(type.Namespace).Append('.');
        }
    }

    private static void AppendName(StringBuilder id, MemberInfo member, bool includeTypeParameterNumber = true)
    {
        var name = includeTypeParameterNumber ? member.Name : member.Name[..^2];
        id.Append(name.Replace('.', '#').Replace('<', '{').Replace('>', '}').Replace(',', '@'));
    }

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