namespace MrKWatkins.Sesharp;

public sealed class RealFileSystem : IFileSystem
{
    public StreamWriter CreateText(string path) => File.CreateText(path);

    public Stream OpenRead(string path) => File.OpenRead(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public void DeleteDirectory(string path, bool recursive) => Directory.Delete(path, recursive);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
}