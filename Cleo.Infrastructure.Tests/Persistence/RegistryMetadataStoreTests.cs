using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Tests.Common;
using Moq;

namespace Cleo.Infrastructure.Tests.Persistence;

public sealed class RegistryMetadataStoreTests
{
    private readonly Mock<ISessionLayout> _layout;
    private readonly Mock<IFileSystem> _fileSystem;
    private readonly RegistryMetadataStore _store;

    public RegistryMetadataStoreTests()
    {
        _layout = new Mock<ISessionLayout>();
        _fileSystem = new Mock<IFileSystem>();
        var provisioner = new DirectorySessionProvisioner(_layout.Object, _fileSystem.Object);
        _store = new RegistryMetadataStore(_layout.Object, _fileSystem.Object, provisioner);
    }

    [Fact]
    public async Task LoadAsync_ReturnsNull_WhenFileDoesNotExist()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("123");
        var path = "/sessions/123/session.json";
        _layout.Setup(x => x.GetMetadataPath(sessionId)).Returns(path);
        _fileSystem.Setup(x => x.FileExists(path)).Returns(false);

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
        var path = "/sessions/123/session.json";
        var json = "{\"SessionId\":\"sessions/123\",\"TaskDescription\":\"Test Task\"}";

        _layout.Setup(x => x.GetMetadataPath(sessionId)).Returns(path);
        _fileSystem.Setup(x => x.FileExists(path)).Returns(true);
        _fileSystem.Setup(x => x.ReadAllTextAsync(path, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _store.LoadAsync(sessionId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("sessions/123", result.SessionId);
        Assert.Equal("Test Task", result.TaskDescription);
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

        var path = "/sessions/123/session.json";
        _layout.Setup(x => x.GetMetadataPath(sessionId)).Returns(path);

        // Act
        await _store.SaveAsync(metadata, CancellationToken.None);

        // Assert
        _fileSystem.Verify(x => x.WriteAllTextAsync(path, It.Is<string>(s => s.Contains("sessions/123")), It.IsAny<CancellationToken>()), Times.Once);
    }
}
