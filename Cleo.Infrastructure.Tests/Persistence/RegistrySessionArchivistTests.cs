using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Tests.Builders;
using Cleo.Tests.Common;
using Moq;
using Xunit;

namespace Cleo.Infrastructure.Tests.Persistence;

public class RegistrySessionArchivistTests
{
    private readonly Mock<ISessionReader> _readerMock = new();
    private readonly Mock<ISessionWriter> _writerMock = new();
    private readonly RegistrySessionArchivist _sut;
    private readonly SessionId _testId = TestFactory.CreateSessionId("test-session");

    public RegistrySessionArchivistTests()
    {
        _sut = new RegistrySessionArchivist(_readerMock.Object, _writerMock.Object);
    }

    [Fact(DisplayName = "GetHistoryAsync should return session log if session exists.")]
    public async Task GetHistoryAsync_ReturnsLog()
    {
        // Arrange
        var activity = new ProgressActivity("act-1", "rem-1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Work");
        var session = new SessionBuilder().WithId(_testId.Value).Build();
        session.AddActivity(activity);

        _readerMock.Setup(r => r.RecallAsync(_testId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.GetHistoryAsync(_testId, null, CancellationToken.None);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains(result, a => a.Id == "act-1");
    }

    [Fact(DisplayName = "GetHistoryAsync should respect criteria.")]
    public async Task GetHistoryAsync_RespectsCriteria()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var activity1 = new ProgressActivity("act-1", "rem-1", now.AddMinutes(-10), ActivityOriginator.Agent, "Old Work");
        var activity2 = new ProgressActivity("act-2", "rem-2", now, ActivityOriginator.Agent, "New Work");
        var session = new SessionBuilder().WithId(_testId.Value).Build();
        session.AddActivity(activity1);
        session.AddActivity(activity2);

        _readerMock.Setup(r => r.RecallAsync(_testId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.GetHistoryAsync(_testId, new HistoryCriteria(Since: now.AddMinutes(-5)), CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("act-2", result[0].Id);
    }

    [Fact(DisplayName = "GetHistoryAsync should return empty list if session does not exist.")]
    public async Task GetHistoryAsync_ReturnsEmptyIfMissing()
    {
        // Arrange
        _readerMock.Setup(r => r.RecallAsync(_testId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        // Act
        var result = await _sut.GetHistoryAsync(_testId, null, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact(DisplayName = "AppendAsync should add new activities and persist session.")]
    public async Task AppendAsync_AddsAndPersists()
    {
        // Arrange
        var session = new SessionBuilder().WithId(_testId.Value).Build();
        _readerMock.Setup(r => r.RecallAsync(_testId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var activity = new ProgressActivity("act-2", "rem-2", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "New Work");
        var activities = new List<SessionActivity> { activity };

        _writerMock.Setup(w => w.RememberAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.AppendAsync(_testId, activities, CancellationToken.None);

        // Assert
        _writerMock.Verify(w => w.RememberAsync(It.Is<Session>(s => s.SessionLog.Any(a => a.Id == "act-2")), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "AppendAsync should not duplicate existing activities.")]
    public async Task AppendAsync_Idempotent()
    {
        // Arrange
        var activity = new ProgressActivity("act-1", "rem-1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Work");
        var session = new SessionBuilder().WithId(_testId.Value).Build();
        session.AddActivity(activity);

        _readerMock.Setup(r => r.RecallAsync(_testId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        await _sut.AppendAsync(_testId, new[] { activity }, CancellationToken.None);

        // Assert
        // Should verify RememberAsync is NOT called or called with same log?
        // Actually, if modified=false, RememberAsync is skipped.
        _writerMock.Verify(w => w.RememberAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "AppendAsync should throw InvalidOperationException if session not found.")]
    public async Task AppendAsync_ThrowsIfMissing()
    {
        // Arrange
        _readerMock.Setup(r => r.RecallAsync(_testId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AppendAsync(_testId, new[] { new ProgressActivity("a", "r", DateTimeOffset.UtcNow, ActivityOriginator.System, "d") }, CancellationToken.None));
    }
}
