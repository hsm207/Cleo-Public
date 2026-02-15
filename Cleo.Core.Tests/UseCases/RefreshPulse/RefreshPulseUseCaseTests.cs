using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Exceptions;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.Tests.Builders;
using Cleo.Core.UseCases.RefreshPulse;
using Xunit;

namespace Cleo.Core.Tests.UseCases.RefreshPulse;

public sealed class RefreshPulseUseCaseTests
{
    private readonly FakePulseMonitor _pulseMonitor = new();
    private readonly FakeActivityClient _activityClient = new();
    private readonly FakeSessionReader _sessionReader = new();
    private readonly FakeSessionWriter _sessionWriter = new();
    private readonly RefreshPulseUseCase _sut;

    public RefreshPulseUseCaseTests()
    {
        _sut = new RefreshPulseUseCase(_pulseMonitor, _activityClient, _sessionReader, _sessionWriter);
    }

    [Fact(DisplayName = "Given a valid Handle, when refreshing the Pulse, then it should retrieve the latest State and History and update the Task Registry.")]
    public async Task ShouldRetrieveLatestPulse()
    {
        // Arrange
        var sessionId = new SessionId("sessions/active-session");

        // Ensure deterministic session state
        var session = new SessionBuilder().WithId("sessions/active-session").Build();
        _sessionReader.Sessions[sessionId] = session;

        // Force the new activity to be explicitly newer than anything in the session
        var newerTimestamp = session.LastActivity.Timestamp.AddHours(1);
        // Important: Must provide a Description (Thought) to make it Significant!
        var activity = new ProgressActivity("act-1", "remote-act-1", newerTimestamp, ActivityOriginator.Agent, "Intent", "Thinking about life...");

        _activityClient.Activities.Add(activity);

        var request = new RefreshPulseRequest(sessionId);

        // Act
        var result = await _sut.ExecuteAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(sessionId, result.Id);
        Assert.Equal(SessionStatus.InProgress, result.Pulse.Status);
        Assert.Equal(SessionState.Working, result.State);
        Assert.Null(result.PullRequest);
        Assert.False(result.IsCached);
        Assert.True(_sessionWriter.Saved);
        
        // Response Fidelity: Ensure LastActivity matches the latest significant activity
        Assert.Equal("act-1", result.LastActivity.Id);

        // Verify history synchronization 🔄📜
        Assert.Contains(session.SessionLog, a => a.Id == "act-1");
    }

    [Fact(DisplayName = "Given the remote collaborator is unreachable, when the Use Case refreshes the Pulse, then it should retrieve the cached State from the local Registry and return it with a connectivity warning.")]
    public async Task ShouldFallbackToCacheOnConnectivityFailure()
    {
        // Arrange
        var sessionId = new SessionId("sessions/active-session");
        var cachedSession = new SessionBuilder()
            .WithId("sessions/active-session")
            .WithPulse(SessionStatus.InProgress)
            .Build();
            
        _sessionReader.Sessions[sessionId] = cachedSession;
        _pulseMonitor.ShouldThrow = true; 

        var request = new RefreshPulseRequest(sessionId);

        // Act
        var result = await _sut.ExecuteAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(sessionId, result.Id);
        Assert.Equal(SessionStatus.InProgress, result.Pulse.Status);
        Assert.True(result.IsCached);
        Assert.NotNull(result.Warning);

        // Response Fidelity: Ensure LastActivity matches the local cached activity
        Assert.Equal(cachedSession.LastActivity, result.LastActivity);
    }

    [Fact(DisplayName = "Given a Handle not in the Registry, when refreshing the Pulse, then it should synchronize and heal the Registry.")]
    public async Task ShouldSynchronizeRecoveredSession()
    {
        // Arrange
        var sessionId = new SessionId("sessions/lost-session");
        var request = new RefreshPulseRequest(sessionId);

        // Act
        var result = await _sut.ExecuteAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(sessionId, result.Id);
        Assert.True(_sessionWriter.Saved);
        Assert.Equal(SessionStatus.InProgress, result.Pulse.Status);
    }

    [Fact(DisplayName = "Given remote session has a PR, when refreshing, then it should sync the PR to local session.")]
    public async Task ShouldSyncPullRequestWhenPresentOnRemote()
    {
        // Arrange
        var sessionId = new SessionId("sessions/pr-session");
        var session = new SessionBuilder().WithId(sessionId.Value).Build();
        _sessionReader.Sessions[sessionId] = session;

        var pr = new PullRequest(new Uri("https://github.com/pr/1"), "Title");

        _pulseMonitor.RemoteSessionConfigurator = remoteSession => {
            remoteSession.SetPullRequest(pr);
        };

        var request = new RefreshPulseRequest(sessionId);

        // Act
        await _sut.ExecuteAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(session.PullRequest);
        Assert.Equal(pr.Title, session.PullRequest.Title);
    }

    [Fact(DisplayName = "Given session is missing locally AND remote is unreachable, when refreshing, then it should throw InvalidOperationException.")]
    public async Task ShouldThrowIfSessionNotFoundLocallyAndRemoteUnreachable()
    {
        // Arrange
        var sessionId = new SessionId("sessions/missing-session");
        _pulseMonitor.ShouldThrow = true;

        var request = new RefreshPulseRequest(sessionId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ExecuteAsync(request, TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Should throw ArgumentNullException if request is null.")]
    public async Task ShouldValidateRequest()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.ExecuteAsync(null!, TestContext.Current.CancellationToken));
    }

    // --- Fakes ---

    private sealed class FakePulseMonitor : IPulseMonitor
    {
        public bool ShouldThrow { get; set; }
        public Action<Session>? RemoteSessionConfigurator { get; set; }

        public Task<Session> GetRemoteSessionAsync(SessionId id, TaskDescription originalTask, CancellationToken cancellationToken = default)
        {
            if (ShouldThrow) throw new RemoteCollaboratorUnavailableException();

            var session = new SessionBuilder()
                .WithId(id.Value)
                .WithTask((string)originalTask)
                .WithPulse(SessionStatus.InProgress)
                .Build();

            RemoteSessionConfigurator?.Invoke(session);

            return Task.FromResult(session);
        }

        public Task<SessionPulse> GetSessionPulseAsync(SessionId id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new SessionPulse(SessionStatus.InProgress));
        }
    }

    private sealed class FakeActivityClient : IJulesActivityClient
    {
        public List<SessionActivity> Activities { get; } = new();
        public Task<IReadOnlyCollection<SessionActivity>> GetActivitiesAsync(SessionId id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<SessionActivity>>(Activities.AsReadOnly());
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
        {
            return Task.FromResult<IReadOnlyCollection<Session>>(Sessions.Values.ToList());
        }
    }

    private sealed class FakeSessionWriter : ISessionWriter
    {
        public bool Saved { get; private set; }
        public Task RememberAsync(Session session, CancellationToken cancellationToken = default)
        {
            Saved = true;
            return Task.CompletedTask;
        }
        public Task ForgetAsync(SessionId id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
