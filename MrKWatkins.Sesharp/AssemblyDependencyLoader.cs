using System.Reflection;

namespace MrKWatkins.Sesharp;

internal static class AssemblyDependencyLoader
{
    internal static IReadOnlyList<Assembly> LoadWithDependencies(IEnumerable<Assembly> assemblies)
    {
        var processed = new HashSet<Assembly>();
        foreach (var assembly in assemblies)
        {
            Load(processed, assembly);
        }
        return [..processed];
    }

    internal static IReadOnlyList<Assembly> LoadWithDependencies(Assembly assembly)
        => LoadWithDependencies([assembly]);

    private static void Load(HashSet<Assembly> processed, Assembly assembly)
    {
        if (!processed.Add(assembly))
        {
            return;
        }

        foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
        {
            Assembly referencedAssembly;
            try
            {
                referencedAssembly = Assembly.Load(referencedAssemblyName);
            }
            catch
            {
                // Assembly.Load(name) won't find assemblies loaded from bytes; check the AppDomain first.
                var alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => AssemblyName.ReferenceMatchesDefinition(referencedAssemblyName, a.GetName()));

                if (alreadyLoaded != null)
                {
                    referencedAssembly = alreadyLoaded;
                }
                else
                {
                    var nugetAssemblyPath = FindAssemblyInNugetCache(referencedAssemblyName);
                    referencedAssembly = Assembly.LoadFrom(nugetAssemblyPath);
                }
            }

            Load(processed, referencedAssembly);
        }
    }

    [Pure]
    private static string FindAssemblyInNugetCache(AssemblyName referencedAssemblyName)
    {
        var nugetCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
        var packageName = referencedAssemblyName.Name!.ToLowerInvariant();
        var packageFolder = Path.Combine(nugetCache, packageName);

        // Try the exact assembly version first.
        var exactVersionFolder = Path.Combine(packageFolder, referencedAssemblyName.Version!.ToString(3));
        if (Directory.Exists(exactVersionFolder))
        {
            var exactPaths = Directory.GetFiles(exactVersionFolder, $"{referencedAssemblyName.Name}.dll", SearchOption.AllDirectories);
            if (exactPaths.Length == 1)
            {
                return exactPaths[0];
            }
        }

        // Assembly version doesn't always match NuGet package version; search all available versions.
        if (Directory.Exists(packageFolder))
        {
            foreach (var versionFolder in Directory.GetDirectories(packageFolder))
            {
                var paths = Directory.GetFiles(versionFolder, $"{referencedAssemblyName.Name}.dll", SearchOption.AllDirectories);
                if (paths.Length == 1)
                {
                    return paths[0];
                }
            }
        }

        throw new InvalidOperationException($"Could not find assembly {referencedAssemblyName.Name} in NuGet cache at {packageFolder}.");
    }
}
