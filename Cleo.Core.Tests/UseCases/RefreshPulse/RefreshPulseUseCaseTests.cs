using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.RefreshPulse;
using Cleo.Core.Tests.Builders;
using Cleo.Tests.Common;
using Xunit;

namespace Cleo.Core.Tests.UseCases.RefreshPulse;

using Cleo.Core.Domain.Services;

public sealed class RefreshPulseUseCaseTests
{
    private readonly FakeSessionReader _reader = new();
    private readonly FakeSessionWriter _writer = new();
    private readonly FakePulseMonitor _monitor = new();
    private readonly FakeActivityClient _activityClient = new();
    private readonly RemoteFirstPrResolver _prResolver = new();
    private readonly RefreshPulseUseCase _sut;

    public RefreshPulseUseCaseTests()
    {
        _sut = new RefreshPulseUseCase(_monitor, _activityClient, _reader, _writer, _prResolver);
    }

    [Fact(DisplayName = "Given a valid Handle, when refreshing the Pulse, then it should retrieve the latest State and History and update the Task Registry.")]
    public async Task ShouldRetrieveLatestPulse()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("active-session");
        var session = new SessionBuilder().WithId(sessionId.Value).Build();
        _reader.Sessions[sessionId] = session;

        var latestPulse = new SessionPulse(SessionStatus.InProgress);
        _monitor.NextPulse = latestPulse;
        _monitor.RemoteSession = new SessionBuilder().WithId(sessionId.Value).WithPulse(SessionStatus.InProgress).Build();

        var request = new RefreshPulseRequest(sessionId);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(sessionId, result.Id);
        Assert.Equal(SessionState.Working, result.State);
        
        // Verify Persistence
        Assert.NotNull(_writer.LastSavedSession);
        Assert.Equal(latestPulse, _writer.LastSavedSession.Pulse);
    }

    [Fact(DisplayName = "Given the remote collaborator is unreachable, when the Use Case refreshes the Pulse, then it should retrieve the cached State from the local Registry and return it with a connectivity warning.")]
    public async Task ShouldFallbackToCacheOnConnectivityFailure()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("active-session");
        var session = new SessionBuilder().WithId(sessionId.Value).WithPulse(SessionStatus.Planning).Build();
        _reader.Sessions[sessionId] = session;

        _monitor.ShouldThrow = true;

        var request = new RefreshPulseRequest(sessionId);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(sessionId, result.Id);
        Assert.Equal(SessionState.Planning, result.State);
        Assert.NotNull(result.Warning);
        Assert.Contains("Remote system unreachable", result.Warning, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Given a Handle not in the Registry, when refreshing the Pulse, then it should synchronize and heal the Registry.")]
    public async Task ShouldSynchronizeRecoveredSession()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("lost-session");
        // Not in Reader!

        var remoteSession = new SessionBuilder().WithId(sessionId.Value).WithPulse(SessionStatus.InProgress).Build();
        _monitor.RemoteSession = remoteSession;

        var request = new RefreshPulseRequest(sessionId);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(sessionId, result.Id);
        Assert.Equal(SessionState.Working, result.State);
        Assert.NotNull(_writer.LastSavedSession);
        Assert.Equal(sessionId, _writer.LastSavedSession.Id);
    }

    [Fact(DisplayName = "Given a session found on Remote but missing locally, when refreshing, it should synchronize the 'Task Description' from the Remote Truth.")]
    public async Task ShouldSynchronizeTaskFromRemoteTruthForRecoveredSessions()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("recovered-identity");
        var remoteTask = "The Authoritative Task";
        var remoteSession = new SessionBuilder()
            .WithId(sessionId.Value)
            .WithTask(remoteTask)
            .Build();
        _monitor.RemoteSession = remoteSession;

        var request = new RefreshPulseRequest(sessionId);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(_writer.LastSavedSession);
        Assert.Equal((TaskDescription)remoteTask, _writer.LastSavedSession.Task);
    }

    [Fact(DisplayName = "Given remote session has a PR, when refreshing, then it should sync the PR to local session.")]
    public async Task ShouldSyncPullRequestWhenPresentOnRemote()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("pr-session");
        var pr = new PullRequest(new Uri("https://pr"), "PR", "Desc", "head", "base");

        var session = new SessionBuilder().WithId(sessionId.Value).Build();
        _reader.Sessions[sessionId] = session;

        var remoteSession = new SessionBuilder().WithId(sessionId.Value).Build();
        remoteSession.SetPullRequest(pr);
        _monitor.RemoteSession = remoteSession; // Return full session with PR

        var request = new RefreshPulseRequest(sessionId);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(pr, result.PullRequest);
        Assert.Equal(pr, _writer.LastSavedSession?.PullRequest);
    }

    [Fact(DisplayName = "Given local session has a PR and remote does not, when refreshing, then it should purge the local PR (Zombie Artifact).")]
    public async Task ShouldPurgeLocalPullRequestWhenRemoteIsMissing()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("local-pr-session");
        var pr = new PullRequest(new Uri("https://pr"), "PR", "Desc", "head", "base");

        var session = new SessionBuilder().WithId(sessionId.Value).Build();
        session.SetPullRequest(pr);
        _reader.Sessions[sessionId] = session;

        var remoteSession = new SessionBuilder().WithId(sessionId.Value).Build();
        // remoteSession has NO PR
        _monitor.RemoteSession = remoteSession;

        var request = new RefreshPulseRequest(sessionId);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Null(result.PullRequest);
        Assert.Null(_writer.LastSavedSession?.PullRequest);
    }

    [Fact(DisplayName = "Given no local session and remote not found, when refreshing, it should throw.")]
    public async Task ShouldThrowWhenSessionNotFoundAnywhere()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("missing-session");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ExecuteAsync(new RefreshPulseRequest(sessionId), CancellationToken.None));
    }

    private sealed class FakeSessionReader : ISessionReader
    {
        public Dictionary<SessionId, Session> Sessions { get; } = new();
        public Task<Session?> RecallAsync(SessionId id, CancellationToken cancellationToken = default) => Task.FromResult(Sessions.GetValueOrDefault(id));
        public Task<IReadOnlyCollection<Session>> ListAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeSessionWriter : ISessionWriter
    {
        public Session? LastSavedSession { get; private set; }
        public Task RememberAsync(Session session, CancellationToken cancellationToken = default)
        {
            LastSavedSession = session;
            return Task.CompletedTask;
        }
        public Task ForgetAsync(SessionId id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakePulseMonitor : IPulseMonitor
    {
        public SessionPulse? NextPulse { get; set; }
        public Session? RemoteSession { get; set; }
        public bool ShouldThrow { get; set; }

        public Task<SessionPulse> GetSessionPulseAsync(SessionId id, CancellationToken cancellationToken = default)
        {
            if (ShouldThrow) throw new Cleo.Core.Domain.Exceptions.RemoteCollaboratorUnavailableException("Fail");
            if (RemoteSession != null) return Task.FromResult(RemoteSession.Pulse);
            return Task.FromResult(NextPulse ?? new SessionPulse(SessionStatus.InProgress));
        }

        public Task<Session> GetRemoteSessionAsync(SessionId id, TaskDescription originalTask, CancellationToken cancellationToken = default)
        {
            if (ShouldThrow) throw new Cleo.Core.Domain.Exceptions.RemoteCollaboratorUnavailableException("Fail");
            if (RemoteSession != null) return Task.FromResult(RemoteSession);
            // If remote session not set, simulate "Not Found" by returning null?
            // The interface contract implies it returns a session or throws.
            // For testing "Not Found", we can throw InvalidOperationException or return null if nullable.
            // Current implementation returns non-nullable Session.
            throw new InvalidOperationException("Remote session not found in mock.");
        }
    }

    private sealed class FakeActivityClient : IJulesActivityClient
    {
        public Task<IReadOnlyCollection<SessionActivity>> GetActivitiesAsync(SessionId id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<SessionActivity>>(Array.Empty<SessionActivity>());
        }
    }
}
