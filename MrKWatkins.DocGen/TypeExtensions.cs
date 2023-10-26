using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MrKWatkins.DocGen;

public static class TypeExtensions
{
    private static readonly ConcurrentDictionary<Type, string> Cache = new();

    [Pure]
    public static string DisplayName(this Type type) =>
        Cache.GetOrAdd(type,
            t => t.IsGenericType
                ? $"{t.Name[..^2]}<{string.Join(", ", t.GetGenericArguments().Select(DisplayName))}>"
                : t.Name);

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