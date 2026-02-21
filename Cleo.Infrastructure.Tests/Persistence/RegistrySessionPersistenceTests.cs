using System.Globalization;
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

    [Fact(DisplayName = "RegistrySession persistence should preserve absolute metadata fidelity (CreatedAt and UpdatedAt).")]
    public async Task Writer_ShouldSave_AndReader_ShouldLoad_Metadata()
    {
        // Arrange
        var id = TestFactory.CreateSessionId("fidelity-check");
        var dashboardUri = new Uri("https://jules.ai/sessions/fidelity");
        
        // Use deterministic, non-current timestamps to prevent "UtcNow" false positives 🏛️💎
        var createdAt = DateTimeOffset.Parse("2026-02-21T20:00:00Z", CultureInfo.InvariantCulture);
        var updatedAt = createdAt.AddMinutes(45);
        
        var session = new Session(
            id, 
            "remote-fid-1", 
            new TaskDescription("Fidelity Test"), 
            TestFactory.CreateSourceContext("repo"), 
            new SessionPulse(SessionStatus.Planning), 
            createdAt, 
            updatedAt: updatedAt, 
            dashboardUri: dashboardUri);

        // Act
        await _writer.RememberAsync(session, CancellationToken.None);
        var result = await _reader.RecallAsync(id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(session.Id, result!.Id);
        Assert.Equal(session.Task, result.Task);
        Assert.Equal(dashboardUri, result.DashboardUri);
        
        // The "High-Fidelity" Assertions 🛡️✨
        Assert.Equal(createdAt, result.CreatedAt); 
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
}
