using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.Entities;
using Cleo.Core.UseCases.BrowseHistory;
using Cleo.Core.Tests.Builders;
using Cleo.Tests.Common;
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
        var sessionId = TestFactory.CreateSessionId("active-session");
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

    [Fact(DisplayName = "Given valid criteria, when browsing history, then it should return filtered results.")]
    public async Task ShouldReturnFilteredHistory()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("filtered-session");
        var now = DateTimeOffset.UtcNow;
        var activity1 = new ProgressActivity("act-1", "rem-1", now.AddMinutes(-10), ActivityOriginator.Agent, "Old");
        var activity2 = new ProgressActivity("act-2", "rem-2", now, ActivityOriginator.Agent, "New");

        _archivist.History[sessionId] = new List<SessionActivity> { activity1, activity2 };
        _sessionReader.Sessions[sessionId] = new SessionBuilder().WithId(sessionId.Value).Build();

        var criteria = new HistoryCriteria(Since: now.AddMinutes(-5));
        var request = new BrowseHistoryRequest(sessionId, criteria);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Single(result.History);
        Assert.Equal("act-2", result.History[0].Id);
    }

    private sealed class FakeArchivist : ISessionArchivist
    {
        public Dictionary<SessionId, List<SessionActivity>> History { get; } = new();

        public Task<IReadOnlyList<SessionActivity>> GetHistoryAsync(SessionId id, HistoryCriteria? criteria = null, CancellationToken cancellationToken = default)
        {
            var activities = History.GetValueOrDefault(id) ?? new List<SessionActivity>();

            if (criteria != null)
            {
                activities = activities.Where(criteria.IsSatisfiedBy).ToList();
            }

            return Task.FromResult<IReadOnlyList<SessionActivity>>(activities);
        }

        public Task AppendAsync(SessionId id, IEnumerable<SessionActivity> activities, CancellationToken cancellationToken = default)
        {
            if (!History.TryGetValue(id, out var list))
            {
                list = new List<SessionActivity>();
                History[id] = list;
            }
            list.AddRange(activities);
            return Task.CompletedTask;
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
