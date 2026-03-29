using System.Reflection;
using System.Text.Json;

namespace MrKWatkins.Sesharp;

internal static class AssemblyDependencyLoader
{
    internal static IReadOnlyList<Assembly> LoadWithDependencies(IEnumerable<Assembly> assemblies, string? assemblyPath = null)
    {
        var depsLookup = assemblyPath != null ? DepsJsonLookup.TryLoad(assemblyPath) : null;

        var processed = new HashSet<Assembly>();
        foreach (var assembly in assemblies)
        {
            Load(processed, assembly, depsLookup);
        }
        return [..processed];
    }

    internal static IReadOnlyList<Assembly> LoadWithDependencies(Assembly assembly, string? assemblyPath = null)
        => LoadWithDependencies([assembly], assemblyPath);

    private static void Load(HashSet<Assembly> processed, Assembly assembly, DepsJsonLookup? depsLookup)
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
                    var assemblyFilePath = FindAssembly(referencedAssemblyName, depsLookup);
                    referencedAssembly = Assembly.LoadFrom(assemblyFilePath);
                }
            }

            Load(processed, referencedAssembly, depsLookup);
        }
    }

    [Pure]
    internal static string FindAssembly(AssemblyName referencedAssemblyName, DepsJsonLookup? depsLookup)
    {
        // Use the deps.json lookup if available; it maps assembly names to the correct
        // NuGet package paths, handling cases where assembly version != package version.
        var resolvedPath = depsLookup?.Resolve(referencedAssemblyName.Name!);
        if (resolvedPath != null)
        {
            return resolvedPath;
        }

        return FindAssemblyInNugetCache(referencedAssemblyName);
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

/// <summary>
/// Parses a .deps.json file to resolve assembly names to full paths in the NuGet cache.
/// </summary>
internal sealed class DepsJsonLookup
{
    private readonly Dictionary<string, string> _assemblyPaths;

    private DepsJsonLookup(Dictionary<string, string> assemblyPaths)
    {
        _assemblyPaths = assemblyPaths;
    }

    [Pure]
    internal string? Resolve(string assemblyName) =>
        _assemblyPaths.GetValueOrDefault(assemblyName);

    [Pure]
    internal static DepsJsonLookup? TryLoad(string assemblyPath)
    {
        var depsPath = assemblyPath.Replace(".dll", ".deps.json", StringComparison.OrdinalIgnoreCase);
        if (!File.Exists(depsPath))
        {
            return null;
        }

        var nugetCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");

        using var stream = File.OpenRead(depsPath);
        using var doc = JsonDocument.Parse(stream);

        var assemblyPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // The "libraries" section maps "PackageName/Version" to a path in the NuGet cache.
        // The "targets" section maps "PackageName/Version" to runtime assets including DLL relative paths.
        if (!doc.RootElement.TryGetProperty("targets", out var targets) ||
            !doc.RootElement.TryGetProperty("libraries", out var libraries))
        {
            return null;
        }

        // Get the first (and usually only) target.
        JsonElement target = default;
        foreach (var t in targets.EnumerateObject())
        {
            target = t.Value;
            break;
        }

        if (target.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        foreach (var library in libraries.EnumerateObject())
        {
            if (!library.Value.TryGetProperty("type", out var typeElement) ||
                typeElement.GetString() != "package")
            {
                continue;
            }

            if (!library.Value.TryGetProperty("path", out var pathElement))
            {
                continue;
            }

            var libraryPath = pathElement.GetString();
            if (libraryPath == null)
            {
                continue;
            }

            // Look up the runtime assets for this library in the target.
            if (!target.TryGetProperty(library.Name, out var targetEntry) ||
                !targetEntry.TryGetProperty("runtime", out var runtime))
            {
                continue;
            }

            foreach (var asset in runtime.EnumerateObject())
            {
                var dllRelativePath = asset.Name;
                var fileName = Path.GetFileNameWithoutExtension(dllRelativePath);
                var fullPath = Path.Combine(nugetCache, libraryPath, dllRelativePath);

                assemblyPaths.TryAdd(fileName, fullPath);
            }
        }

        return new DepsJsonLookup(assemblyPaths);
    }
}
