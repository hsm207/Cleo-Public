using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Infrastructure.Persistence.Mappers;
using Moq;

namespace Cleo.Infrastructure.Tests.Persistence;

public class RegistrySessionPersistenceTests
{
    private readonly string _testPath = "/sessions/registry.json";
    private readonly Mock<IRegistryPathProvider> _pathProviderMock = new();
    private readonly RegistrySessionReader _reader;
    private readonly RegistrySessionWriter _writer;
    private readonly ActivityMapperFactory _activityFactory;
    private readonly FakeFileSystem _fileSystem;

    public RegistrySessionPersistenceTests()
    {
        _pathProviderMock.Setup(p => p.GetRegistryPath()).Returns(_testPath);

        // Register Artifact mapping 🔌📎
        var artifactMapperFactory = new ArtifactMapperFactory(new IArtifactPersistenceMapper[]
        {
            new BashOutputMapper(),
            new ChangeSetMapper(),
            new MediaMapper()
        });

        // Register Activity mapping 🔌🏺
        var activityMappers = new IActivityPersistenceMapper[]
        {
            new Cleo.Infrastructure.Persistence.Mappers.PlanningActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.MessageActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.ApprovalActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.ProgressActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.CompletionActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.FailureActivityMapper(artifactMapperFactory)
        };
        _activityFactory = new ActivityMapperFactory(activityMappers);

        var mapper = new RegistryTaskMapper(_activityFactory);
        var serializer = new JsonRegistrySerializer();
        _fileSystem = new FakeFileSystem();

        _reader = new RegistrySessionReader(_pathProviderMock.Object, mapper, serializer, _fileSystem);
        _writer = new RegistrySessionWriter(_pathProviderMock.Object, mapper, serializer, _fileSystem);
    }

    [Fact(DisplayName = "RegistrySessionWriter should save a session and Reader should retrieve it.")]
    public async Task Writer_ShouldSave_AndReader_ShouldLoad()
    {
        // Arrange
        var id = new SessionId("sessions/real-vibes-1");
        var dashboardUri = new Uri("https://jules.ai/sessions/1");
        var session = new Session(id, "remote-1", new TaskDescription("Real world testing"), new SourceContext("repo", "main"), new SessionPulse(SessionStatus.Planning), DateTimeOffset.UtcNow, dashboardUri: dashboardUri);
        var activity = new ProgressActivity("act-1", "remote-act-1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Initial thought");
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
        Assert.True(_fileSystem.FileExists(_testPath));
        var json = await _fileSystem.ReadAllTextAsync(_testPath, CancellationToken.None);
        Assert.Contains("Real world testing", json);
        Assert.Contains("Initial thought", json);
    }

    [Fact(DisplayName = "RegistrySessionWriter should handle updates to existing sessions.")]
    public async Task Writer_ShouldUpdate_ExistingSession()
    {
        // Arrange
        var id = new SessionId("sessions/update-test");
        var initial = new Session(id, "remote-2", new TaskDescription("Initial"), new SourceContext("r", "b"), new SessionPulse(SessionStatus.StartingUp), DateTimeOffset.UtcNow);
        var updated = new Session(id, "remote-2", new TaskDescription("Updated"), new SourceContext("r", "b"), new SessionPulse(SessionStatus.Completed), DateTimeOffset.UtcNow);

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
        var session = new Session(id, "remote-3", new TaskDescription("Bye bye"), new SourceContext("r", "b"), new SessionPulse(SessionStatus.StartingUp), DateTimeOffset.UtcNow);

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
        await _fileSystem.WriteAllTextAsync(_testPath, "[]", CancellationToken.None);
        result = await _reader.ListAsync(TestContext.Current.CancellationToken);
        Assert.Empty(result);
        
        // Now test whitespace
        await _fileSystem.WriteAllTextAsync(_testPath, "   ", CancellationToken.None);
        result = await _reader.ListAsync(TestContext.Current.CancellationToken);
        Assert.Empty(result);
    }

    [Fact(DisplayName = "RegistrySessionWriter should create directory if it does not exist.")]
    public async Task Writer_ShouldCreateDirectory()
    {
        // Arrange
        var nestedDir = "/sessions/nested";
        var nestedFile = $"{nestedDir}/registry.json";
        var mockPath = new Mock<IRegistryPathProvider>();
        mockPath.Setup(p => p.GetRegistryPath()).Returns(nestedFile);
        var fakeFs = new FakeFileSystem();

        var writer = new RegistrySessionWriter(mockPath.Object, new RegistryTaskMapper(_activityFactory), new JsonRegistrySerializer(), fakeFs);
        var session = new Session(new SessionId("s"), "r", new TaskDescription("t"), new SourceContext("r", "b"), new SessionPulse(SessionStatus.StartingUp), DateTimeOffset.UtcNow);

        // Act
        await writer.RememberAsync(session, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(fakeFs.DirectoryExists(nestedDir));
        Assert.True(fakeFs.FileExists(nestedFile));
    }
}
