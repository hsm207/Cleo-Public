using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Persistence.Internal;
using Moq;

namespace Cleo.Infrastructure.Tests.Persistence;

public class RegistrySessionPersistenceTests
{
    private readonly Mock<IRegistryPathProvider> _pathProviderMock = new();
    private readonly Mock<IRegistryTaskMapper> _mapperMock = new();
    private readonly Mock<IRegistrySerializer> _serializerMock = new();
    private readonly Mock<IFileSystem> _fileSystemMock = new();
    private readonly string _registryPath = "/path/to/registry.json";

    public RegistrySessionPersistenceTests()
    {
        _pathProviderMock.Setup(p => p.GetRegistryPath()).Returns(_registryPath);
    }

    [Fact(DisplayName = "RegistrySessionReader should retrieve a session by ID.")]
    public async Task Reader_ShouldGetById()
    {
        // Arrange
        var id = new SessionId("sessions/123");
        var dto = new RegisteredTaskDto(id.Value, "task", "repo", "branch", "QUEUED", "detail");
        var session = new Session(id, new TaskDescription("task"), new SourceContext("repo", "branch"), new SessionPulse(SessionStatus.StartingUp));
        
        _fileSystemMock.Setup(f => f.FileExists(_registryPath)).Returns(true);
        _fileSystemMock.Setup(f => f.ReadAllTextAsync(_registryPath, It.IsAny<CancellationToken>())).ReturnsAsync("[]");
        _serializerMock.Setup(s => s.Deserialize(It.IsAny<string>())).Returns(new List<RegisteredTaskDto> { dto });
        _mapperMock.Setup(m => m.MapToDomain(dto)).Returns(session);

        var reader = new RegistrySessionReader(_pathProviderMock.Object, _mapperMock.Object, _serializerMock.Object, _fileSystemMock.Object);

        // Act
        var result = await reader.GetByIdAsync(id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(session, result);
    }

    [Fact(DisplayName = "RegistrySessionReader should return empty list when file does not exist.")]
    public async Task Reader_ShouldReturnEmpty_WhenFileMissing()
    {
        // Arrange
        _fileSystemMock.Setup(f => f.FileExists(_registryPath)).Returns(false);
        var reader = new RegistrySessionReader(_pathProviderMock.Object, _mapperMock.Object, _serializerMock.Object, _fileSystemMock.Object);

        // Act
        var result = await reader.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact(DisplayName = "RegistrySessionReader should handle empty or whitespace JSON gracefully.")]
    public async Task Reader_ShouldReturnEmpty_WhenJsonEmpty()
    {
        // Arrange
        _fileSystemMock.Setup(f => f.FileExists(_registryPath)).Returns(true);
        _fileSystemMock.Setup(f => f.ReadAllTextAsync(_registryPath, It.IsAny<CancellationToken>())).ReturnsAsync("   ");
        var reader = new RegistrySessionReader(_pathProviderMock.Object, _mapperMock.Object, _serializerMock.Object, _fileSystemMock.Object);

        // Act
        var result = await reader.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact(DisplayName = "RegistrySessionWriter should save a session and create directory if missing.")]
    public async Task Writer_ShouldSaveSession_AndCreateDirectory()
    {
        // Arrange
        var id = new SessionId("sessions/123");
        var session = new Session(id, new TaskDescription("task"), new SourceContext("repo", "branch"), new SessionPulse(SessionStatus.StartingUp));
        var dto = new RegisteredTaskDto(id.Value, "task", "repo", "branch", "QUEUED", "detail");
        var directory = Path.GetDirectoryName(_registryPath)!;

        _fileSystemMock.Setup(f => f.FileExists(_registryPath)).Returns(false);
        _fileSystemMock.Setup(f => f.DirectoryExists(directory)).Returns(false);
        _mapperMock.Setup(m => m.MapToDto(session)).Returns(dto);
        _serializerMock.Setup(s => s.Serialize(It.IsAny<List<RegisteredTaskDto>>())).Returns("[]");

        var writer = new RegistrySessionWriter(_pathProviderMock.Object, _mapperMock.Object, _serializerMock.Object, _fileSystemMock.Object);

        // Act
        await writer.SaveAsync(session, TestContext.Current.CancellationToken);

        // Assert
        _fileSystemMock.Verify(f => f.CreateDirectory(directory), Times.Once);
        _fileSystemMock.Verify(f => f.WriteAllTextAsync(_registryPath, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "RegistrySessionWriter should delete a session and only save if changes occurred.")]
    public async Task Writer_ShouldDeleteSession_OnlyIfFound()
    {
        // Arrange
        var id = new SessionId("sessions/123");
        var dto = new RegisteredTaskDto(id.Value, "task", "repo", "branch", "QUEUED", "detail");

        // First Case: Found
        _fileSystemMock.Setup(f => f.FileExists(_registryPath)).Returns(true);
        _fileSystemMock.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
        _fileSystemMock.Setup(f => f.ReadAllTextAsync(_registryPath, It.IsAny<CancellationToken>())).ReturnsAsync("[]");
        _serializerMock.Setup(s => s.Deserialize(It.IsAny<string>())).Returns(new List<RegisteredTaskDto> { dto });
        _serializerMock.Setup(s => s.Serialize(It.IsAny<List<RegisteredTaskDto>>())).Returns("[]");

        var writer = new RegistrySessionWriter(_pathProviderMock.Object, _mapperMock.Object, _serializerMock.Object, _fileSystemMock.Object);

        // Act
        await writer.DeleteAsync(id, TestContext.Current.CancellationToken);

        // Assert
        _fileSystemMock.Verify(f => f.WriteAllTextAsync(_registryPath, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

        // Second Case: Not Found
        _fileSystemMock.Invocations.Clear();
        _serializerMock.Setup(s => s.Deserialize(It.IsAny<string>())).Returns(new List<RegisteredTaskDto>());
        
        // Act
        await writer.DeleteAsync(id, TestContext.Current.CancellationToken);

        // Assert
        _fileSystemMock.Verify(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
