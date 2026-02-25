using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using MrKWatkins.Sesharp.Model;

namespace MrKWatkins.Sesharp.SourceLink;

internal sealed class PortablePdbReader : IDisposable
{
    private static readonly Guid SourceLinkGuid = new("CC110556-A091-4D38-9FEC-25AB9A351A6A");

    private readonly MetadataReaderProvider provider;
    private readonly MetadataReader reader;
    private readonly SourceLinkMap? sourceLinkMap;
    private readonly PEReader? peReader;

    private PortablePdbReader(MetadataReaderProvider provider, PEReader? peReader = null)
    {
        this.provider = provider;
        this.peReader = peReader;
        reader = provider.GetMetadataReader();
        sourceLinkMap = ReadSourceLinkMap();
    }

    internal static PortablePdbReader? TryOpen(string assemblyPath)
    {
        // Try external portable PDB file first.
        var pdbPath = Path.ChangeExtension(assemblyPath, ".pdb");
        if (File.Exists(pdbPath))
        {
            try
            {
                // Pre-read into a MemoryStream so the file handle is closed promptly.
                // MetadataReaderProvider takes ownership of the MemoryStream.
                var ms = new MemoryStream(File.ReadAllBytes(pdbPath));
                var provider = MetadataReaderProvider.FromPortablePdbStream(ms);
                return new PortablePdbReader(provider);
            }
            catch
            {
                // Not a portable PDB or failed to open — fall through to embedded.
            }
        }

        // Try embedded PDB.
        if (!File.Exists(assemblyPath))
            return null;

        PEReader? pe = null;
        try
        {
            pe = new PEReader(File.OpenRead(assemblyPath));
            foreach (var entry in pe.ReadDebugDirectory())
            {
                if (entry.Type == DebugDirectoryEntryType.EmbeddedPortablePdb)
                {
                    var embeddedProvider = pe.ReadEmbeddedPortablePdbDebugDirectoryData(entry);
                    return new PortablePdbReader(embeddedProvider, pe);
                }
            }

            pe.Dispose();
        }
        catch
        {
            pe?.Dispose();
        }

        return null;
    }

    private SourceLinkMap? ReadSourceLinkMap()
    {
        try
        {
            foreach (var handle in reader.GetCustomDebugInformation(EntityHandle.ModuleDefinition))
            {
                var info = reader.GetCustomDebugInformation(handle);
                if (reader.GetGuid(info.Kind) == SourceLinkGuid)
                {
                    var bytes = reader.GetBlobBytes(info.Value);
                    var json = Encoding.UTF8.GetString(bytes);
                    return SourceLinkMap.TryParse(json);
                }
            }
        }
        catch
        {
            // Ignore — no SourceLink info available.
        }

        return null;
    }

    internal SourceLocation? GetSourceLocation(MemberInfo memberInfo)
    {
        if (sourceLinkMap == null)
            return null;

        return memberInfo switch
        {
            MethodBase method => GetSourceLocationFromMethod(method),
            PropertyInfo property => GetSourceLocationFromMethod(property.GetMethod ?? property.SetMethod),
            EventInfo @event => GetSourceLocationFromMethod(@event.AddMethod),
            FieldInfo => null,
            System.Type type => GetSourceLocationFromType(type),
            _ => null
        };
    }

    private SourceLocation? GetSourceLocationFromType(System.Type type)
    {
        const BindingFlags binding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        var methods = type.GetConstructors(binding)
            .Concat(type.GetMethods(binding).Cast<MethodBase>());

        foreach (var method in methods)
        {
            var location = GetSourceLocationFromMethod(method);
            if (location != null)
                return location;
        }

        return null;
    }

    private SourceLocation? GetSourceLocationFromMethod(MethodBase? method)
    {
        if (method == null)
            return null;

        try
        {
            var methodDefHandle = MetadataTokens.MethodDefinitionHandle(method.MetadataToken);
            var debugHandle = methodDefHandle.ToDebugInformationHandle();
            var debugInfo = reader.GetMethodDebugInformation(debugHandle);

            if (debugInfo.SequencePointsBlob.IsNil)
                return null;

            foreach (var sp in debugInfo.GetSequencePoints())
            {
                if (sp.IsHidden)
                    continue;

                var document = reader.GetDocument(sp.Document);
                var path = reader.GetString(document.Name);

                var relativePath = sourceLinkMap?.TryMap(path);
                if (relativePath == null)
                    continue;

                return new SourceLocation(relativePath, sp.StartLine);
            }
        }
        catch
        {
            // Ignore — method may not have debug info in this PDB.
        }

        return null;
    }

    public void Dispose()
    {
        provider.Dispose();
        peReader?.Dispose();
    }
}