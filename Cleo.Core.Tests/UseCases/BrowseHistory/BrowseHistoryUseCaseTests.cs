using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.BrowseHistory;
using Xunit;

namespace Cleo.Core.Tests.UseCases.BrowseHistory;

public sealed class BrowseHistoryUseCaseTests
{
    private readonly FakeArchivist _archivist = new();
    private readonly BrowseHistoryUseCase _sut;

    public BrowseHistoryUseCaseTests()
    {
        _sut = new BrowseHistoryUseCase(_archivist);
    }

    [Fact(DisplayName = "Given a valid Handle, when browsing history, then it should return the chronological Session Log containing all Activities.")]
    public async Task ShouldReturnChronologicalHistory()
    {
        // Arrange
        var sessionId = new SessionId("sessions/active-session");
        var activity = new ProgressActivity("act-1", DateTimeOffset.UtcNow, "Did a thing");
        _archivist.History[sessionId] = new List<SessionActivity> { activity };

        var request = new BrowseHistoryRequest(sessionId);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(sessionId, result.Id);
        Assert.Single(result.History);
        Assert.Equal(activity, result.History[0]);
    }

    private sealed class FakeArchivist : ISessionArchivist
    {
        public Dictionary<SessionId, List<SessionActivity>> History { get; } = new();
        public Task<IReadOnlyList<SessionActivity>> GetHistoryAsync(SessionId id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<SessionActivity>>(History.GetValueOrDefault(id) ?? new List<SessionActivity>());
        }
    }
}
