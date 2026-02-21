using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Infrastructure.Persistence.Mappers;
using Cleo.Tests.Common;
using Moq;

namespace Cleo.Infrastructure.Tests.Persistence;

public sealed class RegistryHistoryStoreTests : IDisposable
{
    private readonly TemporaryDirectoryFixture _fixture;
    private readonly DirectorySessionLayout _layout;
    private readonly PhysicalFileSystem _fileSystem;
    private readonly DirectorySessionProvisioner _provisioner;
    private readonly NdjsonActivitySerializer _serializer;
    private readonly RegistryHistoryStore _store;

    public RegistryHistoryStoreTests()
    {
        _fixture = new TemporaryDirectoryFixture();
        var sessionsRoot = Path.Combine(_fixture.DirectoryPath, "sessions");
        Directory.CreateDirectory(sessionsRoot);

        // Real Concretions 🏗️
        _fileSystem = new PhysicalFileSystem();
        var pathResolver = new Cleo.Infrastructure.Tests.Persistence.Internal.TestSessionPathResolver(sessionsRoot);
        _layout = new DirectorySessionLayout(pathResolver);
        _provisioner = new DirectorySessionProvisioner(_layout, _fileSystem);

        // Real Serializer with Mappers 🔌
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
        var mapperFactory = new ActivityMapperFactory(activityMappers);
        _serializer = new NdjsonActivitySerializer(mapperFactory);

        _store = new RegistryHistoryStore(_layout, _fileSystem, _provisioner, _serializer);
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact(DisplayName = "AppendAsync should persist valid NDJSON.")]
    public async Task AppendAsync_PersistsValidNdjson()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("123");
        var activity = new ProgressActivity("act-1", "rem-1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Testing");
        var activities = new[] { activity };

        // Act
        await _store.AppendAsync(sessionId, activities, CancellationToken.None);

        // Assert
        var historyPath = _layout.GetHistoryPath(sessionId);
        Assert.True(File.Exists(historyPath));
        var lines = await File.ReadAllLinesAsync(historyPath);
        Assert.Single(lines);
        Assert.Contains("act-1", lines[0]);
    }

    [Fact(DisplayName = "ReadAsync should robustly ignore empty lines and invalid JSON.")]
    public async Task ReadAsync_IgnoresGarbage()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("123");
        var historyPath = _layout.GetHistoryPath(sessionId);
        _provisioner.EnsureSessionDirectory(sessionId);

        // Construct garbage explicitly with KNOWN valid JSON for valid lines
        // We use the serializer to get "clean" lines for the valid parts to avoid matching errors.
        var valid1 = new ProgressActivity("act-1", "r1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "V1");
        var valid2 = new ProgressActivity("act-2", "r2", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "V2");

        var json1 = _serializer.Serialize(valid1);
        var json2 = _serializer.Serialize(valid2);

        // "garbage": true will be deserialized into ActivityEnvelopeDto with Type=null.
        // If Type is null, ActivityMapperFactory probably throws or fails.
        // We should ensure our "garbage" is actually invalid JSON so JsonSerializer throws JsonException, which NdjsonActivitySerializer catches.

        var content = $"{json1}\n\n   \n{{ invalid json }}\n{json2}";
        await File.WriteAllTextAsync(historyPath, content);

        // Act
        var result = new List<SessionActivity>();
        await foreach (var item in _store.ReadAsync(sessionId, null, CancellationToken.None))
        {
            result.Add(item);
        }

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("act-1", result[0].Id);
        Assert.Equal("act-2", result[1].Id);
    }

    [Fact(DisplayName = "ReadAsync should filter by criteria.")]
    public async Task ReadAsync_FiltersByCriteria()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("123");
        var historyPath = _layout.GetHistoryPath(sessionId);
        _provisioner.EnsureSessionDirectory(sessionId);

        var match = new ProgressActivity("act-1", "r1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Keep me");
        var skip = new ProgressActivity("act-2", "r2", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Skip me");

        var lines = new[] { _serializer.Serialize(match), _serializer.Serialize(skip) };
        await File.WriteAllLinesAsync(historyPath, lines);

        var criteria = new HistoryCriteria(SearchText: "Keep");

        // Act
        var result = new List<SessionActivity>();
        await foreach (var item in _store.ReadAsync(sessionId, criteria, CancellationToken.None))
        {
            result.Add(item);
        }

        // Assert
        Assert.Single(result);
        Assert.Equal("act-1", result[0].Id);
    }

    [Fact(DisplayName = "ReadAsync returns empty if file missing.")]
    public async Task ReadAsync_ReturnsEmpty_IfMissing()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("999"); // Does not exist

        // Act
        var result = new List<SessionActivity>();
        await foreach (var item in _store.ReadAsync(sessionId, null, CancellationToken.None))
        {
            result.Add(item);
        }

        // Assert
        Assert.Empty(result);
    }
}
