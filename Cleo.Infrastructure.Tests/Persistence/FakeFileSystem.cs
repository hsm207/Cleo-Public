using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Tests.Persistence;

public sealed class FakeFileSystem : IFileSystem
{
    private readonly Dictionary<string, string> _files = new();
    private readonly HashSet<string> _directories = new();

    public bool FileExists(string path) => _files.ContainsKey(path);
    public bool DirectoryExists(string path) => _directories.Contains(path);

    public void CreateDirectory(string path) => _directories.Add(path);

    public Task<string> ReadAllTextAsync(string path, CancellationToken ct)
    {
        return Task.FromResult(_files.TryGetValue(path, out var content) ? content : string.Empty);
    }

    public Task WriteAllTextAsync(string path, string content, CancellationToken ct)
    {
        _files[path] = content;

        // Ensure directory is implicitly created for simplicity in tests
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) _directories.Add(dir);

        return Task.CompletedTask;
    }
}
