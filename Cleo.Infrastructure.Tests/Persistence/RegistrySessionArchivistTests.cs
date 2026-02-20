using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Infrastructure.Tests.Builders;
using Cleo.Tests.Common;
using Moq;
using Xunit;

namespace Cleo.Infrastructure.Tests.Persistence;

public class RegistrySessionArchivistTests
{
    private readonly Mock<ISessionReader> _readerMock = new();
    private readonly Mock<ISessionWriter> _writerMock = new();
    private readonly Mock<IHistoryStore> _historyStoreMock = new();
    private readonly RegistrySessionArchivist _sut;
    private readonly SessionId _testId = TestFactory.CreateSessionId("test-session");

    public RegistrySessionArchivistTests()
    {
        _sut = new RegistrySessionArchivist(_readerMock.Object, _writerMock.Object, _historyStoreMock.Object);
    }

    [Fact(DisplayName = "GetHistoryAsync should return session log from history store.")]
    public async Task GetHistoryAsync_ReturnsLog()
    {
        // Arrange
        var activity = new ProgressActivity("act-1", "rem-1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Work");
        var activities = new List<SessionActivity> { activity };

        _historyStoreMock.Setup(h => h.ReadAsync(_testId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);

        // Act
        var result = await _sut.GetHistoryAsync(_testId, null, CancellationToken.None);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains(result, a => a.Id == "act-1");
    }

    [Fact(DisplayName = "GetHistoryAsync should delegate to history store with criteria.")]
    public async Task GetHistoryAsync_DelegatesCriteria()
    {
        // Arrange
        var criteria = new HistoryCriteria(SearchText: "Thinking");

        // Act
        await _sut.GetHistoryAsync(_testId, criteria, CancellationToken.None);

        // Assert
        _historyStoreMock.Verify(h => h.ReadAsync(_testId, criteria, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GetHistoryAsync should return empty list if store returns empty.")]
    public async Task GetHistoryAsync_ReturnsEmptyIfMissing()
    {
        // Arrange
        _historyStoreMock.Setup(h => h.ReadAsync(_testId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<SessionActivity>());

        // Act
        var result = await _sut.GetHistoryAsync(_testId, null, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact(DisplayName = "AppendAsync should append to history store.")]
    public async Task AppendAsync_AppendsToStore()
    {
        // Arrange
        var activity = new ProgressActivity("act-2", "rem-2", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "New Work");
        var activities = new List<SessionActivity> { activity };

        // Act
        await _sut.AppendAsync(_testId, activities, CancellationToken.None);

        // Assert
        _historyStoreMock.Verify(h => h.AppendAsync(_testId, activities, It.IsAny<CancellationToken>()), Times.Once);
    }

}
