namespace Cleo.Infrastructure.Persistence.Internal;

/// <summary>
/// A thin abstraction over the physical file system to enable deterministic testing.
/// </summary>
internal interface IFileSystem
{
    bool FileExists(string path);
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
    Task<string> ReadAllTextAsync(string path, CancellationToken ct);
    Task WriteAllTextAsync(string path, string content, CancellationToken ct);
}

internal sealed class PhysicalFileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);
    public bool DirectoryExists(string path) => Directory.Exists(path);
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    public Task<string> ReadAllTextAsync(string path, CancellationToken ct) => File.ReadAllTextAsync(path, ct);
    public Task WriteAllTextAsync(string path, string content, CancellationToken ct) => File.WriteAllTextAsync(path, content, ct);
}
