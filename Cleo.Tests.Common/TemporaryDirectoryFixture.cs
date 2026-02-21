namespace Cleo.Tests.Common;

/// <summary>
/// A fixture that provides an isolated temporary directory for file-system based tests.
/// Ensures cleanup after tests complete. 🧹🛡️
/// </summary>
public sealed class TemporaryDirectoryFixture : IDisposable
{
    public string DirectoryPath { get; }

    public TemporaryDirectoryFixture()
    {
        DirectoryPath = Path.Combine(Path.GetTempPath(), $"Cleo_Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(DirectoryPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(DirectoryPath))
        {
            try
            {
                Directory.Delete(DirectoryPath, true);
            }
            catch
            {
                // Best effort cleanup. Don't crash tests if a file is locked.
            }
        }
    }
}
