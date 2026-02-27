using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Infrastructure.Persistence.Mappers;
using Cleo.Tests.Common;

namespace Cleo.Infrastructure.Tests.Persistence;

public class RegistrySessionPersistenceTests : IDisposable
{
    private readonly TemporaryDirectoryFixture _fixture;
    private readonly RegistrySessionReader _reader;
    private readonly RegistrySessionWriter _writer;
    private readonly RegistrySessionArchivist _archivist;
    private readonly string _sessionsRoot;
    private readonly PhysicalFileSystem _fileSystem;

    public RegistrySessionPersistenceTests()
    {
        _fixture = new TemporaryDirectoryFixture();
        _sessionsRoot = Path.Combine(_fixture.DirectoryPath, "sessions");
        Directory.CreateDirectory(_sessionsRoot);

        _fileSystem = new PhysicalFileSystem();
        var pathResolver = new Cleo.Infrastructure.Tests.Persistence.Internal.TestSessionPathResolver(_sessionsRoot);
        var layout = new DirectorySessionLayout(pathResolver);
        var provisioner = new DirectorySessionProvisioner(layout, _fileSystem);
        var metadataStore = new RegistryMetadataStore(layout, _fileSystem, provisioner);

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
        var activityFactory = new ActivityMapperFactory(activityMappers);
        var mapper = new RegistryTaskMapper(activityFactory);

        var ndjsonSerializer = new NdjsonActivitySerializer(activityFactory);
        var historyStore = new RegistryHistoryStore(layout, _fileSystem, provisioner, ndjsonSerializer);

        _reader = new RegistrySessionReader(metadataStore, historyStore, mapper, pathResolver, _fileSystem);
        _writer = new RegistrySessionWriter(metadataStore, historyStore, mapper, layout, _fileSystem);
        _archivist = new RegistrySessionArchivist(_reader, _writer, historyStore);
    }

    public void Dispose()
    {
        _fixture.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "The Session Registry should preserve session fidelity, ensuring metadata, history, and complex artifacts are recoverable with 100% accuracy.")]
    public async Task ShouldPreserveSessionFidelityWhenRememberedAndRecalled()
    {
        // Arrange 🏗️
        var id = TestFactory.CreateSessionId("fidelity-1");
        var birthDate = DateTimeOffset.Parse("2024-01-01T12:00:00Z", System.Globalization.CultureInfo.InvariantCulture);
        var activityDate = DateTimeOffset.Parse("2024-01-01T12:30:00Z", System.Globalization.CultureInfo.InvariantCulture);
        var dashboardUri = new Uri("https://jules.ai/sessions/fidelity-1");
        
        var session = new Session(
            id, 
            "remote-1", 
            new TaskDescription("Fidelity Test"), 
            TestFactory.CreateSourceContext("repo"), 
            new SessionPulse(SessionStatus.InProgress), 
            birthDate,
            updatedAt: activityDate,
            dashboardUri: dashboardUri);

        var patch = GitPatch.FromApi("diff content", "sha123");
        var changeSet = new ChangeSet("source", patch);
        var activities = new SessionActivity[]
        {
            new MessageActivity("msg-1", "rem-msg", birthDate.AddMinutes(5), ActivityOriginator.User, "Hello Jules"),
            new ProgressActivity("act-1", "rem-act", activityDate, ActivityOriginator.Agent, "Thinking...", null, new[] { changeSet })
        };

        // Act 🚀
        await _writer.RememberAsync(session, CancellationToken.None);
        await _archivist.AppendAsync(id, activities, CancellationToken.None);
        var result = await _reader.RecallAsync(id, CancellationToken.None);

        // Assert ✅
        Assert.NotNull(result);
        Assert.Equal(session.Id, result!.Id);
        Assert.Equal(session.Task, result.Task);
        Assert.Equal(session.RemoteId, result.RemoteId);
        Assert.Equal(session.CreatedAt, result.CreatedAt);
        Assert.Equal(session.UpdatedAt, result.UpdatedAt);
        Assert.Equal(session.DashboardUri, result.DashboardUri);
        Assert.Equal(session.Pulse.Status, result.Pulse.Status);

        // Assert 3 activities (1 auto-generated SessionAssigned + 2 appended)
        Assert.Equal(3, result.SessionLog.Count);
        
        Assert.Contains(result.SessionLog, a => a is SessionAssignedActivity);
        
        var loadedMsg = result.SessionLog.OfType<MessageActivity>().Single();
        Assert.Equal("Hello Jules", loadedMsg.Text);

        var loadedProgress = result.SessionLog.OfType<ProgressActivity>().Single();
        var loadedChangeSet = loadedProgress.Evidence?.OfType<ChangeSet>().Single();
        Assert.NotNull(loadedChangeSet);
        Assert.Equal(patch.Fingerprint, loadedChangeSet!.Patch.Fingerprint);
        Assert.Equal(patch.UniDiff, loadedChangeSet.Patch.UniDiff);
    }

    [Fact(DisplayName = "The Session Registry should enumerate all locally remembered sessions to provide a complete overview of the developer's workbench.")]
    public async Task ShouldEnumerateAllRememberedSessions()
    {
        // Arrange 🏗️
        var id1 = TestFactory.CreateSessionId("1");
        var id2 = TestFactory.CreateSessionId("2");
        await _writer.RememberAsync(new Session(id1, "r1", new TaskDescription("T1"), TestFactory.CreateSourceContext("repo"), new SessionPulse(SessionStatus.Planning), DateTimeOffset.UtcNow), CancellationToken.None);
        await _writer.RememberAsync(new Session(id2, "r2", new TaskDescription("T2"), TestFactory.CreateSourceContext("repo"), new SessionPulse(SessionStatus.InProgress), DateTimeOffset.UtcNow), CancellationToken.None);

        // Act 🚀
        var results = await _reader.ListAsync(CancellationToken.None);

        // Assert ✅
        Assert.Equal(2, results.Count);
        Assert.Contains(results, s => s.Id == id1);
        Assert.Contains(results, s => s.Id == id2);
    }

    [Fact(DisplayName = "The Session Registry should perform a complete local cleanup when a session is forgotten, removing all traces from the file system.")]
    public async Task ShouldCompletelyForgetSessionDataWhenRequested()
    {
        // Arrange 🏗️
        var id = TestFactory.CreateSessionId("forgotten");
        var session = new Session(id, "r1", new TaskDescription("T"), TestFactory.CreateSourceContext("repo"), new SessionPulse(SessionStatus.Planning), DateTimeOffset.UtcNow);
        await _writer.RememberAsync(session, CancellationToken.None);
        await _archivist.AppendAsync(id, new[] { new MessageActivity("1", "r", DateTimeOffset.UtcNow, ActivityOriginator.User, "Hi") }, CancellationToken.None);

        // Act 🚀
        await _writer.ForgetAsync(id, CancellationToken.None);
        var result = await _reader.RecallAsync(id, CancellationToken.None);

        // Assert ✅
        Assert.Null(result);
        var sessionDir = Path.Combine(_sessionsRoot, "forgotten");
        Assert.False(Directory.Exists(sessionDir));
    }

    [Fact(DisplayName = "The Session Registry should return an empty list when the sessions directory does not exist.")]
    public async Task ShouldReturnEmptyListWhenRegistryDirectoryDoesNotExist()
    {
        // Arrange 🏗️
        // Delete the directory created in constructor
        if (Directory.Exists(_sessionsRoot)) Directory.Delete(_sessionsRoot, true);

        // Act 🚀
        var results = await _reader.ListAsync(CancellationToken.None);

        // Assert ✅
        Assert.Empty(results);
    }

    [Fact(DisplayName = "The Session Registry should ignore invalid folder names when listing sessions to prevent corruption from external files.")]
    public async Task ShouldIgnoreInvalidFolderNamesWhenListingSessions()
    {
        // Arrange 🏗️
        // Create a valid session
        var id = TestFactory.CreateSessionId("valid");
        await _writer.RememberAsync(new Session(id, "r", new TaskDescription("T"), TestFactory.CreateSourceContext("repo"), new SessionPulse(SessionStatus.Planning), DateTimeOffset.UtcNow), CancellationToken.None);

        // Create an invalid folder (not a SessionId)
        var invalidPath = Path.Combine(_sessionsRoot, "not-a-session-id");
        Directory.CreateDirectory(invalidPath);

        // Act 🚀
        var results = await _reader.ListAsync(CancellationToken.None);

        // Assert ✅
        Assert.Single(results);
        Assert.Equal(id, results.First().Id);
    }
}
