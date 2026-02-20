using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Tests.Common;
using Moq;

namespace Cleo.Infrastructure.Tests.Persistence;

public sealed class RegistryHistoryStoreTests
{
    private readonly Mock<ISessionLayout> _layout;
    private readonly Mock<IFileSystem> _fileSystem;
    private readonly DirectorySessionProvisioner _provisioner; // Using real provisioner with mock layout/fs is safer for integration-lite
    private readonly NdjsonActivitySerializer _serializer;
    private readonly RegistryHistoryStore _store;

    public RegistryHistoryStoreTests()
    {
        _layout = new Mock<ISessionLayout>();
        _fileSystem = new Mock<IFileSystem>();
        // Using real provisioner with mocks injected
        _provisioner = new DirectorySessionProvisioner(_layout.Object, _fileSystem.Object);
        // We need a dummy serializer
        var mapperFactory = new Cleo.Infrastructure.Persistence.Mappers.ActivityMapperFactory(new IActivityPersistenceMapper[0]);
        _serializer = new NdjsonActivitySerializer(mapperFactory);

        _store = new RegistryHistoryStore(_layout.Object, _fileSystem.Object, _provisioner, _serializer);
    }

    [Fact(DisplayName = "AppendAsync should NOT read from file system (O(1) Mandate).")]
    public async Task AppendAsync_EnforcesNoReadMandate()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("123");
        var activity = new ProgressActivity("act-1", "rem-1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Testing");
        var activities = new[] { activity };
        var historyPath = "/sessions/123/activities.jsonl";

        _layout.Setup(x => x.GetSessionDirectory(sessionId)).Returns("/sessions/123");
        _layout.Setup(x => x.GetHistoryPath(sessionId)).Returns(historyPath);
        _fileSystem.Setup(x => x.DirectoryExists("/sessions/123")).Returns(true);

        // Act
        await _store.AppendAsync(sessionId, activities, CancellationToken.None);

        // Assert
        // 1. Verify AppendAllLinesAsync WAS called
        _fileSystem.Verify(x => x.AppendAllLinesAsync(historyPath, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Once);

        // 2. Verify ReadAllTextAsync was NEVER called
        _fileSystem.Verify(x => x.ReadAllTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        // 3. Verify ReadLinesAsync was NEVER called
        _fileSystem.Verify(x => x.ReadLinesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
