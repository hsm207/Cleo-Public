using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Infrastructure.Persistence.Mappers;
using Cleo.Tests.Common;
using Moq;

namespace Cleo.Infrastructure.Tests.Persistence;

public class RegistrySessionPersistenceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly RegistrySessionReader _reader;
    private readonly RegistrySessionWriter _writer;
    private readonly ActivityMapperFactory _activityFactory;
    private readonly PhysicalFileSystem _fileSystem;
    private readonly DirectorySessionLayout _layout;
    private readonly Mock<ISessionPathResolver> _resolver;

    public RegistrySessionPersistenceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), $"Cleo_Sessions_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempRoot);

        _resolver = new Mock<ISessionPathResolver>();
        _resolver.Setup(x => x.GetSessionsRoot()).Returns(_tempRoot);

        _fileSystem = new PhysicalFileSystem();
        _layout = new DirectorySessionLayout(_resolver.Object);
        var provisioner = new DirectorySessionProvisioner(_layout, _fileSystem);
        var metadataStore = new RegistryMetadataStore(_layout, _fileSystem, provisioner);

        var artifactMapperFactory = new ArtifactMapperFactory(new IArtifactPersistenceMapper[]
        {
            new BashOutputMapper(),
            new ChangeSetMapper(),
            new MediaMapper()
        });

        var activityMappers = new IActivityPersistenceMapper[]
        {
            new Cleo.Infrastructure.Persistence.Mappers.PlanningActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.MessageActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.ApprovalActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.ProgressActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.CompletionActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.FailureActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.SessionAssignedActivityMapper(artifactMapperFactory)
        };
        _activityFactory = new ActivityMapperFactory(activityMappers);
        var mapper = new RegistryTaskMapper(_activityFactory);

        var ndjsonSerializer = new NdjsonActivitySerializer(_activityFactory);
        var historyStore = new RegistryHistoryStore(_layout, _fileSystem, provisioner, ndjsonSerializer);

        _reader = new RegistrySessionReader(metadataStore, historyStore, mapper, _resolver.Object, _fileSystem);
        _writer = new RegistrySessionWriter(metadataStore, historyStore, mapper, _layout, _fileSystem);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, true);
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Writer_ShouldSave_AndReader_ShouldLoad_Metadata()
    {
        // Arrange
        var id = TestFactory.CreateSessionId("1");
        var dashboardUri = new Uri("https://jules.ai/sessions/1");
        var updatedAt = DateTimeOffset.UtcNow.AddMinutes(5);
        var session = new Session(id, "remote-1", new TaskDescription("Real world testing"), TestFactory.CreateSourceContext("repo"), new SessionPulse(SessionStatus.Planning), DateTimeOffset.UtcNow, updatedAt: updatedAt, dashboardUri: dashboardUri);

        // Act
        await _writer.RememberAsync(session, CancellationToken.None);
        var result = await _reader.RecallAsync(id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(session.Id, result!.Id);
        Assert.Equal(session.Task, result.Task);
        Assert.Equal(dashboardUri, result.DashboardUri);
        Assert.Equal(updatedAt, result.UpdatedAt);
        Assert.Equal(SessionStatus.Planning, result.Pulse.Status);

        // Verify folder structure
        var sessionDir = Path.Combine(_tempRoot, "1");
        Assert.True(Directory.Exists(sessionDir));
        Assert.True(File.Exists(Path.Combine(sessionDir, "session.json")));
    }

    [Fact]
    public async Task ListAsync_ShouldEnumerateSessions()
    {
        // Arrange
        var id1 = TestFactory.CreateSessionId("1");
        var id2 = TestFactory.CreateSessionId("2");
        var session1 = new Session(id1, "remote-1", new TaskDescription("Task 1"), TestFactory.CreateSourceContext("repo"), new SessionPulse(SessionStatus.Planning), DateTimeOffset.UtcNow);
        var session2 = new Session(id2, "remote-2", new TaskDescription("Task 2"), TestFactory.CreateSourceContext("repo"), new SessionPulse(SessionStatus.InProgress), DateTimeOffset.UtcNow);

        await _writer.RememberAsync(session1, CancellationToken.None);
        await _writer.RememberAsync(session2, CancellationToken.None);

        // Act
        var results = await _reader.ListAsync(CancellationToken.None);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, s => s.Id == id1);
        Assert.Contains(results, s => s.Id == id2);
    }

    [Fact]
    public async Task ForgetAsync_ShouldRemoveSessionDirectory()
    {
        // Arrange
        var id = TestFactory.CreateSessionId("1");
        var session = new Session(id, "remote-1", new TaskDescription("Task"), TestFactory.CreateSourceContext("repo"), new SessionPulse(SessionStatus.Planning), DateTimeOffset.UtcNow);
        await _writer.RememberAsync(session, CancellationToken.None);

        // Act
        await _writer.ForgetAsync(id, CancellationToken.None);
        var result = await _reader.RecallAsync(id, CancellationToken.None);

        // Assert
        Assert.Null(result);
        var sessionDir = Path.Combine(_tempRoot, "1");
        Assert.False(Directory.Exists(sessionDir));
    }

    [Fact]
    public async Task Writer_ShouldSave_AndReader_ShouldLoad_History()
    {
        // Arrange
        var id = TestFactory.CreateSessionId("hist-1");
        var session = new Session(id, "remote-1", new TaskDescription("History Test"), TestFactory.CreateSourceContext("repo"), new SessionPulse(SessionStatus.StartingUp), DateTimeOffset.UtcNow);
        var activity = new ProgressActivity("act-1", "rem-1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "First step");
        session.AddActivity(activity);

        // Act
        await _writer.RememberAsync(session, CancellationToken.None);
        var result = await _reader.RecallAsync(id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result!.SessionLog);
        Assert.Contains(result.SessionLog, a => a.Id == "act-1");

        // Verify file
        var historyPath = Path.Combine(_tempRoot, "hist-1", "activities.jsonl");
        Assert.True(File.Exists(historyPath));
        var content = await File.ReadAllTextAsync(historyPath);
        Assert.Contains("First step", content);
    }
}
