using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MrKWatkins.DocGen;

public static class ReflectionExtensions
{
    private static readonly ConcurrentDictionary<MemberInfo, string> Cache = new();

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
    private static string BuildDisplayName(ConstructorInfo constructor) =>
        $"{constructor.DeclaringType!.Name[..^2]}{BuildParameterList(constructor)}";

    [Pure]
    private static string BuildDisplayName(MemberInfo member) => member.Name;

    [Pure]
    private static string BuildDisplayName(MethodInfo method) => $"{method.Name}{BuildParameterList(method)}";

    [Pure]
    private static string BuildDisplayName(Type type)
    {
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
}