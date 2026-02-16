using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.Entities;
using Cleo.Core.UseCases.BrowseHistory;
using Cleo.Core.Tests.Builders;
using Xunit;

namespace Cleo.Core.Tests.UseCases.BrowseHistory;

public sealed class BrowseHistoryUseCaseTests
{
    private readonly FakeArchivist _archivist = new();
    private readonly FakeSessionReader _sessionReader = new();
    private readonly BrowseHistoryUseCase _sut;

    public BrowseHistoryUseCaseTests()
    {
        _sut = new BrowseHistoryUseCase(_archivist, _sessionReader);
    }

    [Fact(DisplayName = "Given a valid Handle, when browsing history, then it should return the chronological Session Log containing all Activities and the Pull Request.")]
    public async Task ShouldReturnChronologicalHistory()
    {
        // Arrange
        var sessionId = new SessionId("sessions/active-session");
        var activity = new ProgressActivity("act-1", "remote-1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Did a thing");
        _archivist.History[sessionId] = new List<SessionActivity> { activity };
        
        var pr = new PullRequest(new Uri("https://github.com/pr/1"), "PR", "desc", "head", "base");
        _sessionReader.Sessions[sessionId] = new SessionBuilder().WithId(sessionId.Value).Build();
        _sessionReader.Sessions[sessionId].SetPullRequest(pr);

        var request = new BrowseHistoryRequest(sessionId);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(sessionId, result.Id);
        Assert.Single(result.History);
        Assert.Equal(activity, result.History[0]);
        Assert.Equal(pr, result.PullRequest);
    }

    private sealed class FakeArchivist : ISessionArchivist
    {
        public Dictionary<SessionId, List<SessionActivity>> History { get; } = new();
        public Task<IReadOnlyList<SessionActivity>> GetHistoryAsync(SessionId id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<SessionActivity>>(History.GetValueOrDefault(id) ?? new List<SessionActivity>());
        }
    }

    private sealed class FakeSessionReader : ISessionReader
    {
        public Dictionary<SessionId, Session> Sessions { get; } = new();
        public Task<Session?> RecallAsync(SessionId id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Sessions.GetValueOrDefault(id));
        }
        public Task<IReadOnlyCollection<Session>> ListAsync(CancellationToken cancellationToken = default) 
            => Task.FromResult<IReadOnlyCollection<Session>>(Sessions.Values.ToList().AsReadOnly());
    }
}
