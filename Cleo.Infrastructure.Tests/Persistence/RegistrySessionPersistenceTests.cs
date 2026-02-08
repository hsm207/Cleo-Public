using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Persistence.Internal;
using Moq;

namespace Cleo.Infrastructure.Tests.Persistence;

public class RegistrySessionPersistenceTests : IDisposable
{
    private readonly string _tempFile;
    private readonly Mock<IRegistryPathProvider> _pathProviderMock = new();
    private readonly RegistrySessionReader _reader;
    private readonly RegistrySessionWriter _writer;

    public RegistrySessionPersistenceTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), $"Cleo_Test_Registry_{Guid.NewGuid():N}.json");
        _pathProviderMock.Setup(p => p.GetRegistryPath()).Returns(_tempFile);

        // REAL VIBES: Use real logic and real FileSystem
        var mapper = new RegistryTaskMapper();
        var serializer = new JsonRegistrySerializer();
        var fileSystem = new PhysicalFileSystem();

        _reader = new RegistrySessionReader(_pathProviderMock.Object, mapper, serializer, fileSystem);
        _writer = new RegistrySessionWriter(_pathProviderMock.Object, mapper, serializer, fileSystem);
    }

    [Fact(DisplayName = "RegistrySessionWriter should save a session and Reader should retrieve it.")]
    public async Task Writer_ShouldSave_AndReader_ShouldLoad()
    {
        // Arrange
        var id = new SessionId("sessions/real-vibes-1");
        var dashboardUri = new Uri("https://jules.ai/sessions/1");
        var session = new Session(id, new TaskDescription("Real world testing"), new SourceContext("repo", "main"), new SessionPulse(SessionStatus.Planning), dashboardUri);
        var activity = new ProgressActivity("act-1", DateTimeOffset.UtcNow, "Initial thought");
        session.AddActivity(activity);

        // Act
        await _writer.RememberAsync(session, TestContext.Current.CancellationToken);
        var result = await _reader.RecallAsync(id, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(session.Id, result!.Id);
        Assert.Equal(session.Task, result.Task);
        Assert.Equal(dashboardUri, result.DashboardUri);
        Assert.Equal(SessionStatus.StartingUp, result.Pulse.Status); // Status is ephemeral! 💓💨
        Assert.Contains(result.SessionLog, a => a.Id == "act-1");
        
        // Verify file actually exists and has content
        Assert.True(File.Exists(_tempFile));
        var json = File.ReadAllText(_tempFile);
        Assert.Contains("Real world testing", json);
        Assert.Contains("Initial thought", json);
    }

    [Fact(DisplayName = "RegistrySessionWriter should handle updates to existing sessions.")]
    public async Task Writer_ShouldUpdate_ExistingSession()
    {
        // Arrange
        var id = new SessionId("sessions/update-test");
        var initial = new Session(id, new TaskDescription("Initial"), new SourceContext("r", "b"), new SessionPulse(SessionStatus.StartingUp));
        var updated = new Session(id, new TaskDescription("Updated"), new SourceContext("r", "b"), new SessionPulse(SessionStatus.Completed));

        // Act
        await _writer.RememberAsync(initial, TestContext.Current.CancellationToken);
        await _writer.RememberAsync(updated, TestContext.Current.CancellationToken);
        var result = await _reader.RecallAsync(id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(SessionStatus.StartingUp, result!.Pulse.Status); // Ephemeral!
        Assert.Equal((TaskDescription)"Updated", result.Task);
    }

    [Fact(DisplayName = "RegistrySessionWriter should delete sessions correctly.")]
    public async Task Writer_ShouldDelete_Session()
    {
        // Arrange
        var id = new SessionId("sessions/delete-me");
        var session = new Session(id, new TaskDescription("Bye bye"), new SourceContext("r", "b"), new SessionPulse(SessionStatus.StartingUp));

        // Act
        await _writer.RememberAsync(session, TestContext.Current.CancellationToken);
        await _writer.ForgetAsync(id, TestContext.Current.CancellationToken);
        var result = await _reader.RecallAsync(id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }

    [Fact(DisplayName = "Reader should return empty when registry file is missing or empty.")]
    public async Task Reader_ShouldHandleMissingFile()
    {
        // Act
        var result = await _reader.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
        
        // Now test empty file
        File.WriteAllText(_tempFile, "[]");
        result = await _reader.ListAsync(TestContext.Current.CancellationToken);
        Assert.Empty(result);
        
        // Now test whitespace
        File.WriteAllText(_tempFile, "   ");
        result = await _reader.ListAsync(TestContext.Current.CancellationToken);
        Assert.Empty(result);
    }

    [Fact(DisplayName = "RegistrySessionWriter should create directory if it does not exist.")]
    public async Task Writer_ShouldCreateDirectory()
    {
        // Arrange
        var nestedDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var nestedFile = Path.Combine(nestedDir, "registry.json");
        var mockPath = new Mock<IRegistryPathProvider>();
        mockPath.Setup(p => p.GetRegistryPath()).Returns(nestedFile);

        var writer = new RegistrySessionWriter(mockPath.Object, new RegistryTaskMapper(), new JsonRegistrySerializer(), new PhysicalFileSystem());
        var session = new Session(new SessionId("s"), new TaskDescription("t"), new SourceContext("r", "b"), new SessionPulse(SessionStatus.StartingUp));

        try
        {
            // Act
            await writer.RememberAsync(session, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(Directory.Exists(nestedDir));
            Assert.True(File.Exists(nestedFile));
        }
        finally
        {
            if (Directory.Exists(nestedDir)) Directory.Delete(nestedDir, true);
        }
    }

    [Fact(DisplayName = "RegistrySessionWriter should have a working DI-compatible constructor.")]
    public void DIConstructor_ShouldWork()
    {
        var mapper = new RegistryTaskMapper();
        var serializer = new JsonRegistrySerializer();
        var fileSystem = new PhysicalFileSystem();
        var pathProvider = new DefaultRegistryPathProvider();

        var writer = new RegistrySessionWriter(pathProvider, mapper, serializer, fileSystem);
        Assert.NotNull(writer);
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile)) File.Delete(_tempFile);
    }
}
