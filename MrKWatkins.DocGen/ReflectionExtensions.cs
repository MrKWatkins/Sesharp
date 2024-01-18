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
        if (type.IsArray)
        {
            var elementType = BuildDisplayName(type.GetElementType()!);
            var rank = new string(',', type.GetArrayRank() - 1);
            return $"{elementType}[{rank}]";
        }

        if (type.IsByRef)
        {
            return BuildDisplayName(type.GetElementType()!);
        }

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
    public static bool IsPublicOrProtected(this FieldInfo field) => field.IsPublic || field.IsProtected();

    [Pure]
    public static bool IsProtected(this FieldInfo field) => field.IsFamily || field.IsFamilyOrAssembly;

    [Pure]
    public static bool IsPublicOrProtected(this MethodBase method) => method.IsPublic || method.IsProtected();

    [Pure]
    public static bool IsProtected(this MethodBase method) => method.IsFamily || method.IsFamilyOrAssembly;

    [Pure]
    public static bool IsPublic(this PropertyInfo property) =>
        (property.GetMethod != null && property.GetMethod.IsPublic) ||
        (property.SetMethod != null && property.SetMethod.IsPublic);

    [Pure]
    public static bool IsProtected(this PropertyInfo property) =>
        (property.GetMethod != null && property.GetMethod.IsProtected()) ||
        (property.SetMethod != null && property.SetMethod.IsProtected());

    [Pure]
    public static bool IsPublicOrProtected(this PropertyInfo property) =>
        (property.GetMethod != null && property.GetMethod.IsPublicOrProtected()) ||
        (property.SetMethod != null && property.SetMethod.IsPublicOrProtected());

    [Pure]
    public static bool IsPublic(this EventInfo @event) => @event.AddMethod?.IsPublic == true;

    [Pure]
    public static bool IsProtected(this EventInfo @event) => @event.AddMethod?.IsProtected() == true;

    [Pure]
    public static bool IsPublicOrProtected(this EventInfo @event) => @event.AddMethod?.IsPublicOrProtected() == true;

    [Pure]
    public static bool HasInitSetter(this PropertyInfo propertyInfo) =>
        propertyInfo.SetMethod?.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit)) == true;

    [Pure]
    public static bool IsRequired(this PropertyInfo propertyInfo) => propertyInfo.IsDefined(typeof(RequiredMemberAttribute));

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

    [Pure]
    public static ParameterKind? GetKind(this ParameterInfo parameter)
    {
        if (parameter.IsIn)
        {
            return ParameterKind.In;
        }

        if (parameter.IsOut)
        {
            return ParameterKind.Out;
        }

        if (parameter.GetCustomAttribute<ParamArrayAttribute>() != null)
        {
            return ParameterKind.Params;
        }

        if (parameter.ParameterType.IsByRef)
        {
            return ParameterKind.Ref;
        }

        return null;
    }

    [Pure]
    public static string ToKeyword(this ParameterKind parameterKind) =>
        parameterKind switch
        {
            ParameterKind.Params => "params",
            ParameterKind.Ref => "ref",
            ParameterKind.Out => "out",
            ParameterKind.In => "in",
            _ => throw new NotSupportedException($"The {nameof(ParameterKind)} {parameterKind} is not supported.")
        };

    [Pure]
    public static bool IsIndexer(this PropertyInfo propertyInfo) => propertyInfo.GetIndexParameters().Length > 0;

    [Pure]
    public static bool HasPublicOrProtectedOverloads(this MethodBase method) =>
        method switch
        {
            ConstructorInfo constructorInfo => constructorInfo.HasPublicOrProtectedOverloads(),
            MethodInfo methodInfo => methodInfo.HasPublicOrProtectedOverloads(),
            _ => throw new NotSupportedException($"Methods of type {method.GetType().DisplayName()} are not supported.")
        };

    [Pure]
    public static bool HasPublicOrProtectedOverloads(this MethodInfo method)
    {
        var type = method.DeclaringType!;
        var methods = type
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(m => m != method && m.Name == method.Name && m.IsPublicOrProtected());
        return methods.Any();
    }

    [Pure]
    public static bool HasPublicOrProtectedOverloads(this ConstructorInfo constructor)
    {
        var type = constructor.DeclaringType!;
        var constructors = type
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(c => c != constructor && c.IsPublicOrProtected());
        return constructors.Any();
    }

    [Pure]
    public static bool HasPublicOrProtectedOverloads(this PropertyInfo property)
    {
        if (!property.IsIndexer())
        {
            return false;
        }

        var type = property.DeclaringType!;
        var indexers = type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(p => p != property && p.IsIndexer() && (PropertyAccessorIsPublicOrProtected(p.GetMethod) || PropertyAccessorIsPublicOrProtected(p.SetMethod)));
        return indexers.Any();
    }

    [Pure]
    private static bool PropertyAccessorIsPublicOrProtected(MethodInfo? accessor) => accessor == null || accessor.IsPublicOrProtected();
}