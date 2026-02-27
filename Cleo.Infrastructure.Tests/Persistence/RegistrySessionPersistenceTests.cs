using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Infrastructure.Persistence.Mappers;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using Cleo.Tests.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Tests.Persistence;

public class RegistrySessionPersistenceTests : IDisposable
{
    private readonly TemporaryDirectoryFixture _fixture;
    private readonly RegistrySessionReader _reader;
    private readonly RegistrySessionWriter _writer;
    private readonly RegistrySessionArchivist _archivist;
    private readonly string _sessionsRoot;
    private readonly PhysicalFileSystem _fileSystem;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

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
            new Cleo.Infrastructure.Persistence.Mappers.PlanningActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.MessageActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.ApprovalActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.ProgressActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.CompletionActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.FailureActivityMapper(artifactMapperFactory),
            new Cleo.Infrastructure.Persistence.Mappers.SessionAssignedActivityMapper(artifactMapperFactory)
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

    [Fact(DisplayName = "The Session Registry should preserve exhaustive session fidelity, ensuring every activity and nested artifact is recoverable with 100% accuracy.")]
    public async Task ShouldPreserveExhaustiveSessionFidelityWhenRememberedAndRecalled()
    {
        // Arrange 🏗️
        var id = TestFactory.CreateSessionId("fidelity-1");
        var birthDate = DateTimeOffset.Parse("2024-01-01T12:00:00Z", System.Globalization.CultureInfo.InvariantCulture);
        var activityDate = DateTimeOffset.Parse("2024-01-01T12:30:00Z", System.Globalization.CultureInfo.InvariantCulture);
        var dashboardUri = new Uri("https://jules.ai/sessions/fidelity-1");
        
        var session = new Session(
            id, 
            "remote-1", 
            new TaskDescription("Exhaustive Fidelity Test"), 
            TestFactory.CreateSourceContext("repo"), 
            new SessionPulse(SessionStatus.InProgress), 
            birthDate,
            updatedAt: activityDate,
            dashboardUri: dashboardUri);

        // Load exhaustive activities from [internal metadata: TestData scrubbed] 💎🎁
        var activitiesPath = Path.Combine("[internal metadata: TestData scrubbed]", "Jules", "activities_list.json");
        var activitiesJson = File.ReadAllText(activitiesPath);
        var activitiesDto = JsonSerializer.Deserialize<JulesListActivitiesResponseDto>(activitiesJson, JsonOptions)!;
        
        var julesMapper = new CompositeJulesActivityMapper(new IJulesActivityMapper[]
        {
            new Cleo.Infrastructure.Clients.Jules.Mapping.PlanningActivityMapper(), 
            new Cleo.Infrastructure.Clients.Jules.Mapping.UserMessageActivityMapper(),
            new Cleo.Infrastructure.Clients.Jules.Mapping.AgentMessageActivityMapper(),
            new Cleo.Infrastructure.Clients.Jules.Mapping.ProgressActivityMapper(),
            new Cleo.Infrastructure.Clients.Jules.Mapping.CompletionActivityMapper(),
            new Cleo.Infrastructure.Clients.Jules.Mapping.FailureActivityMapper(),
            new Cleo.Infrastructure.Clients.Jules.Mapping.ApprovalActivityMapper()
        });

        var activities = activitiesDto.Activities.Select(a => julesMapper.Map(a)).ToArray();

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

        // Assert count (1 auto-generated SessionAssigned + loaded rich activities)
        Assert.Equal(activities.Length + 1, result.SessionLog.Count);
        Assert.Contains(result.SessionLog, a => a is SessionAssignedActivity);
        
        foreach (var original in activities)
        {
            var loaded = result.SessionLog.Single(a => a.RemoteId == original.RemoteId);
            Assert.Equal(original.Id, loaded.Id);     
            Assert.Equal(original.Originator, loaded.Originator);
            Assert.Equal(original.Timestamp, loaded.Timestamp);
            Assert.Equal(original.ExecutiveSummary, loaded.ExecutiveSummary);
            
            // Nested Fidelity Checks (Elevated from scraps) 🏺📜💎
            if (original is MessageActivity originalMsg && loaded is MessageActivity loadedMsg)
            {
                Assert.Equal(originalMsg.Text, loadedMsg.Text);
            }
            
            if (original is PlanningActivity originalPlan && loaded is PlanningActivity loadedPlan)
            {
                Assert.Equal(originalPlan.PlanId, loadedPlan.PlanId);
                Assert.Equal(originalPlan.Steps.Count, loadedPlan.Steps.Count);
                for (int i = 0; i < originalPlan.Steps.Count; i++)
                {
                    Assert.Equal(originalPlan.Steps.ElementAt(i).Id, loadedPlan.Steps.ElementAt(i).Id);
                    Assert.Equal(originalPlan.Steps.ElementAt(i).Title, loadedPlan.Steps.ElementAt(i).Title);
                    Assert.Equal(originalPlan.Steps.ElementAt(i).Description, loadedPlan.Steps.ElementAt(i).Description);
                }
            }

            if (original is ProgressActivity originalProg && loaded is ProgressActivity loadedProg)
            {
                Assert.Equal(originalProg.Evidence?.Count ?? 0, loadedProg.Evidence?.Count ?? 0);
                if (originalProg.Evidence != null)
                {
                    foreach (var originalArt in originalProg.Evidence)
                    {
                        var loadedArt = loadedProg.Evidence!.Single(a => a.GetType() == originalArt.GetType() && a.GetSummary() == originalArt.GetSummary());
                        if (originalArt is ChangeSet originalCs && loadedArt is ChangeSet loadedCs)
                        {
                            Assert.Equal(originalCs.Patch.Fingerprint, loadedCs.Patch.Fingerprint);
                            Assert.Equal(originalCs.Patch.UniDiff, loadedCs.Patch.UniDiff);
                        }
                        if (originalArt is BashOutput originalBash && loadedArt is BashOutput loadedBash)
                        {
                            Assert.Equal(originalBash.Command, loadedBash.Command);
                            Assert.Equal(originalBash.Output, loadedBash.Output);
                            Assert.Equal(originalBash.ExitCode, loadedBash.ExitCode);
                        }
                    }
                }
            }
        }

        // Discriminator Stability Check 🧱
        var historyPath = Path.Combine(_sessionsRoot, "fidelity-1", "activities.jsonl");
        var historyLines = File.ReadAllLines(historyPath);
        Assert.Contains(historyLines, l => l.Contains("\"Type\":\"PLAN_GENERATED\"", StringComparison.Ordinal));
        Assert.Contains(historyLines, l => l.Contains("\"Type\":\"PROGRESS\"", StringComparison.Ordinal));
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

    [Fact(DisplayName = "The Session Registry should handle missing root directories gracefully by returning an empty session list.")]
    public async Task ShouldReturnEmptyListWhenRegistryDirectoryDoesNotExist()
    {
        // Arrange 🏗️
        if (Directory.Exists(_sessionsRoot)) Directory.Delete(_sessionsRoot, true);

        // Act 🚀
        var results = await _reader.ListAsync(CancellationToken.None);

        // Assert ✅
        Assert.Empty(results);
    }

    [Fact(DisplayName = "The Session Registry should throw an exception when discovering a session folder that lacks a metadata file, exposing registry corruption.")]
    public async Task ShouldThrowWhenMetadataIsMissingForDiscoveredSession()
    {
        // Arrange 🏗️
        var roguePath = Path.Combine(_sessionsRoot, "rogue-session");
        Directory.CreateDirectory(roguePath);

        // Act & Assert ✅
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _reader.ListAsync(CancellationToken.None));
        Assert.Contains("Registry integrity violation", ex.Message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "The Session Registry should allow retrieving raw history through the Archivist for high-efficiency audit trails.")]
    public async Task ShouldRetrieveHistoryUsingTheArchivist()
    {
        // Arrange 🏗️
        var id = TestFactory.CreateSessionId("archivist-1");
        await _writer.RememberAsync(new Session(id, "r", new TaskDescription("T"), TestFactory.CreateSourceContext("repo"), new SessionPulse(SessionStatus.Planning), DateTimeOffset.UtcNow), CancellationToken.None);
        
        var activities = new[] { new MessageActivity("1", "r1", DateTimeOffset.UtcNow, ActivityOriginator.User, "Hi") };
        await _archivist.AppendAsync(id, activities, CancellationToken.None);

        // Act 🚀
        var history = await _archivist.GetHistoryAsync(id, null, CancellationToken.None);

        // Assert ✅
        Assert.Single(history);
        Assert.Equal("Hi", ((MessageActivity)history[0]).Text);
    }

    [Fact(DisplayName = "The Session Registry should support filtering history through criteria to enable targeted narrative analysis.")]
    public async Task ShouldFilterHistoryUsingCriteria()
    {
        // Arrange 🏗️
        var id = TestFactory.CreateSessionId("filter-1");
        await _writer.RememberAsync(new Session(id, "r", new TaskDescription("T"), TestFactory.CreateSourceContext("repo"), new SessionPulse(SessionStatus.Planning), DateTimeOffset.UtcNow), CancellationToken.None);
        
        var now = DateTimeOffset.UtcNow;
        var activities = new SessionActivity[] 
        { 
            new MessageActivity("1", "r1", now, ActivityOriginator.User, "User said something"),
            new ProgressActivity("2", "r2", now.AddMinutes(1), ActivityOriginator.Agent, "Agent thinking")
        };
        await _archivist.AppendAsync(id, activities, CancellationToken.None);

        // Filter for only Progress activities
        var criteria = new HistoryCriteria(ActivityTypes: new[] { typeof(ProgressActivity) });

        // Act 🚀
        var filtered = await _archivist.GetHistoryAsync(id, criteria, CancellationToken.None);

        // Assert ✅
        Assert.Single(filtered);
        Assert.IsType<ProgressActivity>(filtered[0]);
    }
}
