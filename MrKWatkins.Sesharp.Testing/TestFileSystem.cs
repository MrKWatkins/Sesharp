using System.Text;

namespace MrKWatkins.Sesharp.Testing;

public sealed class TestFileSystem : IFileSystem
{
    private readonly Dictionary<string, CreatedFile> createdFiles = new();

    public StreamWriter CreateText(string path)
    {
        var memoryStream = new MemoryStream();

        // TODO: Remove this.
        if (!createdFiles.ContainsKey(path))
        {
            createdFiles.Add(path, new CreatedFile(memoryStream));
        }

        return new StreamWriter(memoryStream, leaveOpen: true);
    }

    public IReadOnlyDictionary<string, CreatedFile> CreatedFiles => createdFiles;

    public Stream OpenRead(string path) =>
        typeof(TestFileSystem).Assembly.GetManifestResourceStream(typeof(TestFileSystem), $"Resources.{path}")
        ?? throw new IOException($"File {path} not found.");

    public void DeleteDirectory(string path, bool recursive)
    {
        throw new NotImplementedException();
    }

    public void CreateDirectory(string path)
    {
        throw new NotImplementedException();
    }

    public sealed class CreatedFile(MemoryStream stream)
    {
        public byte[] Bytes => stream.ToArray();

        public string Text => Encoding.UTF8.GetString(Bytes);
    }
}