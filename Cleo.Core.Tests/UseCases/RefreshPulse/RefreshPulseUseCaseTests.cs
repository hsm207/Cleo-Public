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
    private readonly FakeActivitySource _activitySource = new();
    private readonly RemoteFirstPrResolver _prResolver = new();
    private readonly RefreshPulseUseCase _sut;

    public RefreshPulseUseCaseTests()
    {
        _sut = new RefreshPulseUseCase(_monitor, _activitySource, _reader, _writer, _prResolver);
    }

    [Fact(DisplayName = "Given a null request, when executing, then it should throw ArgumentNullException.")]
    public async Task ShouldThrowIfRequestIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.ExecuteAsync(null!, CancellationToken.None));
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
        // Fix: Ensure remote session is present to avoid InvalidOperationException in mock
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

    [Fact(DisplayName = "Given a session missing from local registry, when refreshing, then it should recover identity and perform a full initial sync (Since=null).")]
    public async Task ShouldRecoverMissingSessionWithFullInitialSync()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("recovered-session");
        var remoteTask = "The Authoritative Task";

        // Remote Session exists with State and Task
        var remoteSession = new SessionBuilder()
            .WithId(sessionId.Value)
            .WithTask(remoteTask)
            .WithPulse(SessionStatus.InProgress)
            .Build();
        _monitor.RemoteSession = remoteSession;

        // Remote has history (e.g. 5 activities)
        var remoteActivities = Enumerable.Range(1, 5)
            .Select(i => new ProgressActivity($"rem-{i}", $"rem-{i}", DateTimeOffset.UtcNow.AddMinutes(i), ActivityOriginator.Agent, $"Step {i}"))
            .Cast<SessionActivity>()
            .ToList();
        _activitySource.ActivitiesToReturn = remoteActivities;

        // Local Registry is Empty (implicit via _reader default state for this ID)

        var request = new RefreshPulseRequest(sessionId);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        // 1. Session Identity Recovered
        Assert.Equal(sessionId, result.Id);
        Assert.NotNull(_writer.LastSavedSession);
        Assert.Equal(sessionId, _writer.LastSavedSession.Id);

        // 2. Task Synced
        Assert.Equal((TaskDescription)remoteTask, _writer.LastSavedSession.Task);

        // 3. Initial Sync Performed (Since = null)
        Assert.Null(_activitySource.LastOptions?.Since);

        // 4. Log Hydrated
        Assert.Equal(5, _writer.LastSavedSession.SessionLog.Count(a => a.Id.StartsWith("rem-", StringComparison.Ordinal)));
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
        _monitor.ShouldThrow = true; // Ensure RemoteCollaboratorUnavailableException is caught, triggering the "not found" check.

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ExecuteAsync(new RefreshPulseRequest(sessionId), CancellationToken.None));
    }

    [Fact(DisplayName = "Given a solution exists but PR is missing, when refreshing, then it should flag HasUnsubmittedSolution.")]
    public async Task ShouldIndicateUnsubmittedSolution()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("ghost-session");
        var remoteSession = new SessionBuilder().WithId(sessionId.Value).Build();
        // Add a "Solution" via a GitPatch output (CompletionActivity)
        var patch = new GitPatch("diff", "sha");
        var changeSet = new ChangeSet("repo", patch);
        var completion = new CompletionActivity("out-1", "rem-1", DateTimeOffset.UtcNow, ActivityOriginator.System, new[] { changeSet });

        remoteSession.AddActivity(completion);
        // Ensure no PR is set on remoteSession (it is null by default in builder unless SetPullRequest is called)

        _monitor.RemoteSession = remoteSession;

        var request = new RefreshPulseRequest(sessionId);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.HasUnsubmittedSolution);
        Assert.Null(result.PullRequest);
    }

    [Fact(DisplayName = "Given existing history, when refreshing, then it should only fetch and append new activities since the last local timestamp.")]
    public async Task ShouldNotDuplicateExistingActivities()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("duplicate-check");
        // Ensure timestamps are identical for value equality if used, but ID check should suffice
        var now = DateTimeOffset.UtcNow;
        var existingActivity = new ProgressActivity("act-1", "rem-1", now, ActivityOriginator.Agent, "Existing");
        var newActivity = new ProgressActivity("act-2", "rem-2", now.AddMinutes(1), ActivityOriginator.Agent, "New");

        // The SessionBuilder creates a session with an initial "SessionAssignedActivity" (Zero-Hollow Invariant)
        // So session.SessionLog count is 1.
        var session = new SessionBuilder().WithId(sessionId.Value).Build();
        session.AddActivity(existingActivity); // Now count is 2
        _reader.Sessions[sessionId] = session;

        var remoteSession = new SessionBuilder().WithId(sessionId.Value).Build();
        _monitor.RemoteSession = remoteSession;

        // Mock activity client returns 'existingActivity' (which is already in local session) and 'newActivity'
        // Incremental fetch will ask for >= 'now'.
        // Mock will filter and return existing (>= now) and new (> now).
        _activitySource.ActivitiesToReturn = new List<SessionActivity> { existingActivity, newActivity };

        var request = new RefreshPulseRequest(sessionId);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(_writer.LastSavedSession);
        var logs = _writer.LastSavedSession.SessionLog;

        // Verify incremental fetching was used
        Assert.NotNull(_activitySource.LastOptions?.Since);
        Assert.Equal(now, _activitySource.LastOptions?.Since);

        // Expected:
        // 1. Initial "Session Assigned" (from builder)
        // 2. "Existing" (added manually)
        // 3. "New" (synced from client)
        // "Existing" from client should be skipped.
        // Total = 3.

        Assert.Contains(logs, a => a.Id == "act-1");
        Assert.Contains(logs, a => a.Id == "act-2");

        // Verify no duplicates of act-1
        Assert.Equal(1, logs.Count(a => a.Id == "act-1"));

        // Verify total count (Initial + Existing + New)
        Assert.Equal(3, logs.Count);
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

    private sealed class FakeActivitySource : IRemoteActivitySource
    {
        public List<SessionActivity> ActivitiesToReturn { get; set; } = new();
        public RemoteActivityOptions? LastOptions { get; private set; }

        public Task<IReadOnlyCollection<SessionActivity>> FetchActivitiesAsync(SessionId id, RemoteActivityOptions options, CancellationToken cancellationToken = default)
        {
            LastOptions = options;
            // Simple mock filtering to ensure we respect "Since" in tests if needed
            if (options.Since.HasValue)
            {
                return Task.FromResult<IReadOnlyCollection<SessionActivity>>(
                    ActivitiesToReturn.Where(a => a.Timestamp >= options.Since.Value).ToList());
            }
            return Task.FromResult<IReadOnlyCollection<SessionActivity>>(ActivitiesToReturn);
        }
    }
}
