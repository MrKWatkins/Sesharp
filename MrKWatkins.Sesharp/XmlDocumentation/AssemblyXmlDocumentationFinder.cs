using System.Reflection;

namespace MrKWatkins.Sesharp.XmlDocumentation;

internal static class AssemblyXmlDocumentationFinder
{
    [Pure]
    public static string? FindXmlPath(Assembly assembly)
    {
        var location = assembly.Location;
        if (string.IsNullOrEmpty(location))
        {
            return null;
        }

        // Try next to the DLL (works for NuGet packages and build output).
        var xmlPath = Path.ChangeExtension(location, ".xml");
        if (File.Exists(xmlPath))
        {
            return xmlPath;
        }

        // Try the matching file in the .NET SDK reference packs for BCL assemblies.
        var refPackDir = FindReferencePackDirectory(location);
        if (refPackDir == null)
        {
            return null;
        }

        var assemblyFileName = Path.GetFileNameWithoutExtension(location);
        var refPackXmlPath = Path.Combine(refPackDir, assemblyFileName + ".xml");
        return File.Exists(refPackXmlPath) ? refPackXmlPath : null;
    }

    // Returns the reference pack directory for a runtime assembly (e.g.
    // /usr/share/dotnet/packs/Microsoft.NETCore.App.Ref/10.0.3/ref/net10.0), or null
    // if the assembly is not in a dotnet/shared/ directory.
    [Pure]
    public static string? FindReferencePackDirectory(Assembly assembly) =>
        string.IsNullOrEmpty(assembly.Location) ? null : FindReferencePackDirectory(assembly.Location);

    [Pure]
    private static string? FindReferencePackDirectory(string assemblyLocation)
    {
        // Assembly location pattern: {dotnetRoot}/shared/{frameworkName}/{version}/{assembly}.dll
        // Reference pack pattern:    {dotnetRoot}/packs/{frameworkName}.Ref/{version}/ref/net{major}.{minor}/
        var assemblyDir = Path.GetDirectoryName(assemblyLocation);
        if (assemblyDir == null)
            return null;

        var versionString = Path.GetFileName(assemblyDir);

        var frameworkDir = Path.GetDirectoryName(assemblyDir);
        if (frameworkDir == null)
            return null;

        var frameworkName = Path.GetFileName(frameworkDir);

        var sharedDir = Path.GetDirectoryName(frameworkDir);
        if (sharedDir == null || !Path.GetFileName(sharedDir).Equals("shared", StringComparison.OrdinalIgnoreCase))
            return null;

        var dotnetRoot = Path.GetDirectoryName(sharedDir);
        if (dotnetRoot == null)
            return null;

        if (!Version.TryParse(versionString, out var version))
            return null;

        var tfm = $"net{version.Major}.{version.Minor}";
        var refPackName = frameworkName + ".Ref";

        var refPackDir = Path.Combine(dotnetRoot, "packs", refPackName, versionString, "ref", tfm);
        return Directory.Exists(refPackDir) ? refPackDir : null;
    }
}