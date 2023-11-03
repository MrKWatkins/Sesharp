using System.Reflection;

namespace MrKWatkins.DocGen;

public sealed class TypeLookup
{
    private readonly IReadOnlyDictionary<string, (Type Type, TypeLocation Location)> types;

    public TypeLookup(Assembly assembly)
    {
        types = GetAllAssemblies(assembly)
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t.Namespace != null)
            .ToDictionary(
                t => t.FullName!,
                t => (t, GetTypeLocation(assembly, t)));
    }

    [Pure]
    private static TypeLocation GetTypeLocation(Assembly documentationAssembly, Type type)
    {
        if (type.Assembly == documentationAssembly)
        {
            return TypeLocation.DocumentAssembly;
        }

        return type.Namespace!.StartsWith("System", StringComparison.OrdinalIgnoreCase)
            ? TypeLocation.System
            : TypeLocation.Other;
    }

    [Pure]
    private static IEnumerable<Assembly> GetAllAssemblies(Assembly assembly) => GetAllAssemblies(new HashSet<Assembly>(), assembly);

    [Pure]
    private static IEnumerable<Assembly> GetAllAssemblies(HashSet<Assembly> processed, Assembly assembly)
    {
        if (processed.Contains(assembly))
        {
            yield break;
        }

        yield return assembly;

        processed.Add(assembly);

        foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
        {
            var referencedAssembly = Assembly.Load(referencedAssemblyName);

            foreach (var child in GetAllAssemblies(processed, referencedAssembly))
            {
                yield return child;
            }
        }
    }

    [Pure]
    public (Type Type, TypeLocation Location) Get(string typeName) => types[typeName];
}