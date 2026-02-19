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

    [Fact(DisplayName = "GetHistoryAsync should respect filtering criteria (Type, Time, Text).")]
    public async Task GetHistoryAsync_RespectsAllCriteria()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var planning = new PlanningActivity("plan-1", "rem-1", now.AddMinutes(-10), ActivityOriginator.Agent, new PlanId("plan-1"), Array.Empty<PlanStep>());
        var progressOld = new ProgressActivity("prog-1", "rem-2", now.AddMinutes(-5), ActivityOriginator.Agent, "Thinking about life");
        var progressNew = new ProgressActivity("prog-2", "rem-3", now, ActivityOriginator.Agent, "Executing plan");

        var session = new SessionBuilder().WithId(_testId.Value).Build();
        session.AddActivity(planning);
        session.AddActivity(progressOld);
        session.AddActivity(progressNew);

        _readerMock.Setup(r => r.RecallAsync(_testId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // 1. Filter by Type
        var typeResult = await _sut.GetHistoryAsync(_testId, new HistoryCriteria(ActivityTypes: new[] { typeof(PlanningActivity) }), CancellationToken.None);
        Assert.Single(typeResult);
        Assert.IsType<PlanningActivity>(typeResult[0]);

        // 2. Filter by Time (Since)
        var sinceResult = await _sut.GetHistoryAsync(_testId, new HistoryCriteria(Since: now.AddMinutes(-2)), CancellationToken.None);
        Assert.Single(sinceResult);
        Assert.Equal("prog-2", sinceResult[0].Id);

        // 3. Filter by Time (Until)
        // Note: We expect 2 items here because the SessionAssignedActivity (created by Builder at -60m)
        // also satisfies the condition (< -8m).
        var untilResult = await _sut.GetHistoryAsync(_testId, new HistoryCriteria(Until: now.AddMinutes(-8)), CancellationToken.None);
        Assert.Equal(2, untilResult.Count);
        Assert.Contains(untilResult, a => a.Id == "plan-1");
        Assert.Contains(untilResult, a => a is SessionAssignedActivity);

        // 4. Filter by Text
        var textResult = await _sut.GetHistoryAsync(_testId, new HistoryCriteria(SearchText: "Thinking"), CancellationToken.None);
        Assert.Single(textResult);
        Assert.Equal("prog-1", textResult[0].Id);
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
