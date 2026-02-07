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
    private readonly string _tempPath;

    public RegistrySessionPersistenceTests()
    {
        _tempPath = Path.GetTempFileName();
        _pathProviderMock.Setup(p => p.GetRegistryPath()).Returns(_tempPath);
    }

    [Fact(DisplayName = "RegistrySessionReader should retrieve a session by ID.")]
    public async Task Reader_ShouldGetById()
    {
        // Arrange
        var id = new SessionId("sessions/123");
        var dto = new RegisteredTaskDto(id.Value, "task", "repo", "branch", "QUEUED", "detail");
        var session = new Session(id, new TaskDescription("task"), new SourceContext("repo", "branch"), new SessionPulse(SessionStatus.StartingUp));
        
        File.WriteAllText(_tempPath, "[]");
        _serializerMock.Setup(s => s.Deserialize(It.IsAny<string>())).Returns(new List<RegisteredTaskDto> { dto });
        _mapperMock.Setup(m => m.MapToDomain(dto)).Returns(session);

        var reader = new RegistrySessionReader(_pathProviderMock.Object, _mapperMock.Object, _serializerMock.Object);

        // Act
        var result = await reader.GetByIdAsync(id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(session, result);
    }

    [Fact(DisplayName = "RegistrySessionWriter should save a session.")]
    public async Task Writer_ShouldSaveSession()
    {
        // Arrange
        var id = new SessionId("sessions/123");
        var session = new Session(id, new TaskDescription("task"), new SourceContext("repo", "branch"), new SessionPulse(SessionStatus.StartingUp));
        var dto = new RegisteredTaskDto(id.Value, "task", "repo", "branch", "QUEUED", "detail");

        File.WriteAllText(_tempPath, "[]");
        _serializerMock.Setup(s => s.Deserialize(It.IsAny<string>())).Returns(new List<RegisteredTaskDto>());
        _mapperMock.Setup(m => m.MapToDto(session)).Returns(dto);
        _serializerMock.Setup(s => s.Serialize(It.IsAny<List<RegisteredTaskDto>>())).Returns("[]");

        var writer = new RegistrySessionWriter(_pathProviderMock.Object, _mapperMock.Object, _serializerMock.Object);

        // Act
        await writer.SaveAsync(session, TestContext.Current.CancellationToken);

        // Assert
        _serializerMock.Verify(s => s.Serialize(It.Is<List<RegisteredTaskDto>>(l => l.Contains(dto))), Times.Once);
    }

    [Fact(DisplayName = "RegistrySessionWriter should delete a session.")]
    public async Task Writer_ShouldDeleteSession()
    {
        // Arrange
        var id = new SessionId("sessions/123");
        var dto = new RegisteredTaskDto(id.Value, "task", "repo", "branch", "QUEUED", "detail");

        File.WriteAllText(_tempPath, "[]");
        _serializerMock.Setup(s => s.Deserialize(It.IsAny<string>())).Returns(new List<RegisteredTaskDto> { dto });
        _serializerMock.Setup(s => s.Serialize(It.IsAny<List<RegisteredTaskDto>>())).Returns("[]");

        var writer = new RegistrySessionWriter(_pathProviderMock.Object, _mapperMock.Object, _serializerMock.Object);

        // Act
        await writer.DeleteAsync(id, TestContext.Current.CancellationToken);

        // Assert
        _serializerMock.Verify(s => s.Serialize(It.Is<List<RegisteredTaskDto>>(l => l.Count == 0)), Times.Once);
    }
}
