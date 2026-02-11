using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Infrastructure.Persistence.Mappers;
using Moq;

namespace Cleo.Infrastructure.Tests.Persistence;

public class RegistrySessionPersistenceTests : IDisposable
{
    private readonly string _tempFile;
    private readonly Mock<IRegistryPathProvider> _pathProviderMock = new();
    private readonly RegistrySessionReader _reader;
    private readonly RegistrySessionWriter _writer;
    private readonly ActivityMapperFactory _activityFactory;
    private readonly PhysicalFileSystem _fileSystem;

    public RegistrySessionPersistenceTests()
    {
        // Use a unique file path for each test instance to ensure isolation 🌍✨
        _tempFile = Path.Combine(Path.GetTempPath(), $"Cleo_Test_Registry_{Guid.NewGuid():N}.json");
        _pathProviderMock.Setup(p => p.GetRegistryPath()).Returns(_tempFile);

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
        _fileSystem = new PhysicalFileSystem(); // Use the REAL file system as demanded! 💎

        _reader = new RegistrySessionReader(_pathProviderMock.Object, mapper, serializer, _fileSystem);
        _writer = new RegistrySessionWriter(_pathProviderMock.Object, mapper, serializer, _fileSystem);
    }

    public void Dispose()
    {
        // Clean up the fixture 🧹
        if (File.Exists(_tempFile))
        {
            File.Delete(_tempFile);
        }
        GC.SuppressFinalize(this);
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
        Assert.True(File.Exists(_tempFile));
        var json = await File.ReadAllTextAsync(_tempFile);
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
        await File.WriteAllTextAsync(_tempFile, "[]");
        result = await _reader.ListAsync(TestContext.Current.CancellationToken);
        Assert.Empty(result);
        
        // Now test whitespace
        await File.WriteAllTextAsync(_tempFile, "   ");
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

        var writer = new RegistrySessionWriter(mockPath.Object, new RegistryTaskMapper(_activityFactory), new JsonRegistrySerializer(), new PhysicalFileSystem());
        var session = new Session(new SessionId("s"), "r", new TaskDescription("t"), new SourceContext("r", "b"), new SessionPulse(SessionStatus.StartingUp), DateTimeOffset.UtcNow);

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

    [Fact(DisplayName = "PhysicalFileSystem should delegate to System.IO.")]
    public async Task PhysicalFileSystem_DelegatesCorrectly()
    {
        var fs = new PhysicalFileSystem();
        var path = Path.GetTempFileName();
        try
        {
            await fs.WriteAllTextAsync(path, "Hello", CancellationToken.None);
            Assert.True(fs.FileExists(path));
            Assert.Equal("Hello", await fs.ReadAllTextAsync(path, CancellationToken.None));

            // Check directory delegation
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            fs.CreateDirectory(dir);
            Assert.True(fs.DirectoryExists(dir));
            Directory.Delete(dir);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
