using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MrKWatkins.DocGen;

public static class TypeExtensions
{
    private static readonly ConcurrentDictionary<Type, string> Cache = new();

    [Pure]
    public static string DisplayName(this Type type) => Cache.GetOrAdd(type, BuildDisplayName);

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
    public static bool IsRecord(this Type type)
    {
        if (!type.IsClass)
        {
            return false;
        }

        return type.GetProperty("EqualityContract", BindingFlags.Instance | BindingFlags.NonPublic)?
            .GetCustomAttribute(typeof(CompilerGeneratedAttribute)) != null;
    }
}