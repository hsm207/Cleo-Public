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
    private readonly TemporaryDirectoryFixture _fixture;
    private readonly RegistrySessionReader _reader;
    private readonly RegistrySessionWriter _writer;
    private readonly RegistrySessionArchivist _archivist;

    // We retain mocks for things OUTSIDE the scope of persistence (like specific activity mapping logic if needed),
    // but for the persistence mechanism itself, we use concretions.
    // However, ActivityMapperFactory is complex, so using the real one is best for "Integration" feel.
    private readonly ActivityMapperFactory _activityFactory;

    public RegistrySessionPersistenceTests()
    {
        _fixture = new TemporaryDirectoryFixture();
        var sessionsRoot = Path.Combine(_fixture.DirectoryPath, "sessions");
        Directory.CreateDirectory(sessionsRoot);

        // Real Concretions 🏗️
        var fileSystem = new PhysicalFileSystem();
        var pathResolver = new Cleo.Infrastructure.Tests.Persistence.Internal.TestSessionPathResolver(sessionsRoot);
        var layout = new DirectorySessionLayout(pathResolver);
        var provisioner = new DirectorySessionProvisioner(layout, fileSystem);
        var metadataStore = new RegistryMetadataStore(layout, fileSystem, provisioner);

        // Real Mappers 🔌
        var artifactMapperFactory = new ArtifactMapperFactory(new IArtifactPersistenceMapper[]
        {
            new BashOutputMapper(),
            new ChangeSetMapper(),
            new MediaMapper()
        });

        var activityMappers = new IActivityPersistenceMapper[]
        {
            new PlanningActivityMapper(artifactMapperFactory),
            new MessageActivityMapper(artifactMapperFactory),
            new ApprovalActivityMapper(artifactMapperFactory),
            new ProgressActivityMapper(artifactMapperFactory),
            new CompletionActivityMapper(artifactMapperFactory),
            new FailureActivityMapper(artifactMapperFactory),
            new SessionAssignedActivityMapper(artifactMapperFactory)
        };
        _activityFactory = new ActivityMapperFactory(activityMappers);
        var mapper = new RegistryTaskMapper(_activityFactory);

        var ndjsonSerializer = new NdjsonActivitySerializer(_activityFactory);
        var historyStore = new RegistryHistoryStore(layout, fileSystem, provisioner, ndjsonSerializer);

        _reader = new RegistrySessionReader(metadataStore, historyStore, mapper, pathResolver, fileSystem);
        _writer = new RegistrySessionWriter(metadataStore, historyStore, mapper, layout, fileSystem);
        _archivist = new RegistrySessionArchivist(_reader, _writer, historyStore);
    }

    public void Dispose()
    {
        _fixture.Dispose();
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
        var sessionDir = Path.Combine(_fixture.DirectoryPath, "sessions", "1");
        Assert.False(Directory.Exists(sessionDir));
    }

    [Fact]
    public async Task Archivist_ShouldAppend_AndReader_ShouldLoad_History()
    {
        // Arrange
        var id = TestFactory.CreateSessionId("hist-1");
        var session = new Session(id, "remote-1", new TaskDescription("History Test"), TestFactory.CreateSourceContext("repo"), new SessionPulse(SessionStatus.StartingUp), DateTimeOffset.UtcNow);
        var activity = new ProgressActivity("act-1", "rem-1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "First step");
        
        // Initial setup
        await _writer.RememberAsync(session, CancellationToken.None);

        // Act
        await _archivist.AppendAsync(id, new[] { activity }, CancellationToken.None);
        var result = await _reader.RecallAsync(id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result!.SessionLog);
        Assert.Contains(result.SessionLog, a => a.Id == "act-1");
    }

    [Fact]
    public async Task Archivist_ShouldPersist_ChangeSet_With_Fingerprint_And_Timestamp_Fidelity()
    {
        // Arrange
        var birthDate = DateTimeOffset.Parse("2024-01-01T12:00:00Z");
        var activityDate = DateTimeOffset.Parse("2024-01-01T12:30:00Z");

        var id = TestFactory.CreateSessionId("cs-fp");
        var session = new Session(id, "remote-fp", new TaskDescription("Fingerprint Test"), TestFactory.CreateSourceContext("repo"), new SessionPulse(SessionStatus.InProgress), birthDate);
        await _writer.RememberAsync(session, CancellationToken.None);

        var patch = GitPatch.FromApi("diff content", "sha123");
        var changeSet = new ChangeSet("source", patch);
        var activity = new ProgressActivity("act-fp", "rem-fp", activityDate, ActivityOriginator.Agent, "Made changes", null, new[] { changeSet });

        // Act
        await _archivist.AppendAsync(id, new[] { activity }, CancellationToken.None);
        var result = await _reader.RecallAsync(id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);

        // Assert Metadata Fidelity
        Assert.Equal(birthDate, result!.CreatedAt);
        Assert.Equal("remote-fp", result.RemoteId);

        // Assert Activity & Fingerprint Fidelity
        var loadedActivity = result.SessionLog.OfType<ProgressActivity>().FirstOrDefault(a => a.Id == "act-fp");
        Assert.NotNull(loadedActivity);
        Assert.Equal(activityDate, loadedActivity!.Timestamp);

        var evidence = loadedActivity.Evidence?.First();
        Assert.NotNull(evidence);

        Assert.IsType<ChangeSet>(evidence);
        var loadedChangeSet = (ChangeSet)evidence!;
        Assert.Equal(patch.Fingerprint, loadedChangeSet.Patch.Fingerprint);
    }
}
