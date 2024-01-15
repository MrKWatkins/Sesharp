using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MrKWatkins.DocGen;

public static class ReflectionExtensions
{
    private static readonly ConcurrentDictionary<MemberInfo, string> Cache = new();

    private static readonly FrozenDictionary<Type, string> TypeKeywords = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(bool?), "bool?" },
            { typeof(char), "char" },
            { typeof(char?), "char?" },
            { typeof(decimal), "decimal" },
            { typeof(decimal?), "decimal?" },
            { typeof(double), "double" },
            { typeof(double?), "double?" },
            { typeof(float), "float" },
            { typeof(float?), "float?" },
            { typeof(int), "int" },
            { typeof(int?), "int?" },
            { typeof(long), "long" },
            { typeof(long?), "long?" },
            { typeof(nint), "nint" },
            { typeof(nint?), "nint?" },
            { typeof(nuint), "nuint" },
            { typeof(nuint?), "nuint?" },
            { typeof(object), "object" },
            { typeof(sbyte), "sbyte" },
            { typeof(sbyte?), "sbyte?" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(uint?), "uint?" },
            { typeof(ulong), "ulong" },
            { typeof(ulong?), "ulong?" },
            { typeof(ushort), "ushort" },
            { typeof(ushort?), "ushort?" },
            { typeof(void), "void" }
        }
        .ToFrozenDictionary();

    [Pure]
    public static string DisplayNameOrKeyword(this Type type) => TypeKeywords.GetValueOrDefault(type) ?? DisplayName(type);

    [Pure]
    public static string DisplayName(this MemberInfo member) => Cache.GetOrAdd(member, m =>
        m switch
        {
            ConstructorInfo constructor => BuildDisplayName(constructor),
            EventInfo @event => BuildDisplayName(@event),
            FieldInfo field => BuildDisplayName(field),
            MethodInfo method => BuildDisplayName(method),
            PropertyInfo property => BuildDisplayName(property),
            Type type => BuildDisplayName(type),
            _ => throw new NotSupportedException($"Members of type {member.GetType().DisplayName()} are not supported.")
        });

    [Pure]
    private static string BuildDisplayName(ConstructorInfo constructor)
    {
        var type = constructor.DeclaringType!;
        if (type.IsGenericType)
        {
            return $"{constructor.DeclaringType!.Name[..^2]}{BuildParameterList(constructor)}";
        }
        return $"{constructor.DeclaringType!.Name}{BuildParameterList(constructor)}";
    }

    [Pure]
    private static string BuildDisplayName(MemberInfo member) => member.Name;

    [Pure]
    private static string BuildDisplayName(MethodInfo method) => $"{method.Name}{BuildParameterList(method)}";

    [Pure]
    private static string BuildDisplayName(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
        {
            return $"{BuildDisplayName(underlyingType)}?";
        }

        if (type.IsGenericParameter)
        {
            if (type.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Contravariant))
            {
                return $"in {type.Name}";
            }

            if (type.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Covariant))
            {
                return $"out {type.Name}";
            }

            return type.Name;
        }

        return type.IsGenericType
            ? $"{type.Name[..^2]}<{string.Join(", ", type.GetGenericArguments().Select(DisplayName))}>"
            : type.Name;
    }

    [Pure]
    private static string BuildParameterList(MethodBase method) =>
        $"({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.DisplayName()))})";

    [Pure]
    public static bool IsRecord(this Type type)
    {
        if (!type.IsClass)
        {
            return false;
        }

        return type.GetProperty("EqualityContract", BindingFlags.Instance | BindingFlags.NonPublic)?
            .GetCustomAttribute(typeof(CompilerGeneratedAttribute)) != null;
    }

    /// <summary>
    /// If a type is a nested type then it enumerates its parents, starting at the outermost type, followed by the type itself.
    /// If it is not nested then it just returns the type.
    /// </summary>
    public static IEnumerable<Type> EnumerateNestedTypes(this Type type)
    {
        if (type.IsNested)
        {
            foreach (var parent in EnumerateNestedTypes(type.DeclaringType!))
            {
                yield return parent;
            }
        }

        yield return type;
    }

    [Pure]
    public static Visibility GetVisibility(this MethodBase method) => method.IsPublic ? Visibility.Public : Visibility.Protected;

    [Pure]
    public static string ToKeyword(this Visibility visibility) =>
        visibility switch
        {
            Visibility.Protected => "protected",
            Visibility.Public => "public",
            _ => throw new NotSupportedException($"The {nameof(Visibility)} {visibility} is not supported.")
        };

    [Pure]
    public static Virtuality? GetVirtuality(this MethodInfo method)
    {
        var isNew = method.IsNew();
        if (method.IsAbstract)
        {
            return isNew ? Virtuality.NewAbstract : Virtuality.Abstract;
        }

        if (method.GetBaseDefinition() != method)
        {
            return method.IsFinal ? Virtuality.SealedOverride : Virtuality.Override;
        }

        if (method is { IsVirtual: true, IsFinal: false })
        {
            return isNew ? Virtuality.NewVirtual : Virtuality.Virtual;
        }

        return isNew ? Virtuality.New : null;
    }

    [Pure]
    public static bool IsNew(this MethodInfo method)
    {
        if (method.GetBaseDefinition() != method)
        {
            return false;
        }

        var baseType = method.DeclaringType?.BaseType;
        while (baseType != null && baseType != typeof(object))
        {
            var sameMethod = baseType.GetMethod(method.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            if (sameMethod != null)
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    [Pure]
    public static string ToKeyword(this Virtuality virtuality) =>
        virtuality switch
        {
            Virtuality.Abstract => "abstract",
            Virtuality.Virtual => "virtual",
            Virtuality.Override => "override",
            Virtuality.SealedOverride => "sealed override",
            Virtuality.New => "new",
            Virtuality.NewAbstract => "new abstract",
            Virtuality.NewVirtual => "new virtual",
            _ => throw new NotSupportedException($"The {nameof(Virtuality)} {virtuality} is not supported.")
        };
}