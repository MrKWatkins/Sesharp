namespace MrKWatkins.Sesharp;

public interface IFileSystem
{
    [MustDisposeResource]
    StreamWriter CreateText([PathReference] string path);

    [MustDisposeResource]
    Stream OpenRead([PathReference] string path);

    bool DirectoryExists([PathReference] string path);

    void DeleteDirectory([PathReference] string path, bool recursive);

    void CreateDirectory([PathReference] string path);
}