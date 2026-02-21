using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Tests.Common;
using Moq;

namespace Cleo.Infrastructure.Tests.Persistence;

public sealed class RegistryMetadataStoreTests : IDisposable
{
    private readonly TemporaryDirectoryFixture _fixture;
    private readonly DirectorySessionLayout _layout;
    private readonly PhysicalFileSystem _fileSystem;
    private readonly DirectorySessionProvisioner _provisioner;
    private readonly RegistryMetadataStore _store;

    public RegistryMetadataStoreTests()
    {
        _fixture = new TemporaryDirectoryFixture();
        var sessionsRoot = Path.Combine(_fixture.DirectoryPath, "sessions");
        Directory.CreateDirectory(sessionsRoot);

        _fileSystem = new PhysicalFileSystem();
        var pathResolver = new Cleo.Infrastructure.Tests.Persistence.Internal.TestSessionPathResolver(sessionsRoot);
        _layout = new DirectorySessionLayout(pathResolver);
        _provisioner = new DirectorySessionProvisioner(_layout, _fileSystem);
        _store = new RegistryMetadataStore(_layout, _fileSystem, _provisioner);
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public async Task LoadAsync_ReturnsNull_WhenFileDoesNotExist()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("123");

        // Act
        var result = await _store.LoadAsync(sessionId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoadAsync_ReturnsMetadata_WhenFileExists()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("123");
        var metadata = new SessionMetadataDto(
            "sessions/123",
            "Test Task",
            "user/repo",
            "main",
            Core.Domain.ValueObjects.SessionStatus.StartingUp,
            null);

        await _store.SaveAsync(metadata, CancellationToken.None);

        // Act
        var result = await _store.LoadAsync(sessionId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("sessions/123", result!.SessionId);
        Assert.Equal("Test Task", result.TaskDescription);
    }

    [Fact]
    public async Task LoadAsync_ReturnsNull_WhenJsonIsCorrupt()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("123");
        var path = _layout.GetMetadataPath(sessionId);
        _provisioner.EnsureSessionDirectory(sessionId);
        await File.WriteAllTextAsync(path, "{ garbage }");

        // Act
        var result = await _store.LoadAsync(sessionId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SaveAsync_WritesMetadata()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("123");
        var metadata = new SessionMetadataDto(
            "sessions/123",
            "Test Task",
            "user/repo",
            "main",
            Core.Domain.ValueObjects.SessionStatus.StartingUp,
            null);

        // Act
        await _store.SaveAsync(metadata, CancellationToken.None);

        // Assert
        var path = _layout.GetMetadataPath(sessionId);
        Assert.True(File.Exists(path));
        var content = await File.ReadAllTextAsync(path);
        Assert.Contains("sessions/123", content);
    }
}
