namespace Cleo.Infrastructure.Persistence.Internal;

/// <summary>
/// A thin abstraction over the physical file system to enable deterministic testing.
/// </summary>
public interface IFileSystem
{
    bool FileExists(string path);
    bool DirectoryExists(string path);
    IEnumerable<string> EnumerateDirectories(string path);
    void CreateDirectory(string path);
    void DeleteDirectory(string path, bool recursive);
    Task<string> ReadAllTextAsync(string path, CancellationToken ct);
    Task WriteAllTextAsync(string path, string content, CancellationToken ct);
    Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken ct);
}

internal sealed class PhysicalFileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);
    public bool DirectoryExists(string path) => Directory.Exists(path);
    public IEnumerable<string> EnumerateDirectories(string path) => Directory.EnumerateDirectories(path);
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    public void DeleteDirectory(string path, bool recursive) => Directory.Delete(path, recursive);
    public Task<string> ReadAllTextAsync(string path, CancellationToken ct) => File.ReadAllTextAsync(path, ct);
    public Task WriteAllTextAsync(string path, string content, CancellationToken ct) => File.WriteAllTextAsync(path, content, ct);
    public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken ct) => File.AppendAllLinesAsync(path, contents, ct);
}
