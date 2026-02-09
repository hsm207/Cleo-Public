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
        var session = new SessionBuilder().WithId("sessions/active-session").Build();
        _sessionReader.Sessions[sessionId] = session;

        var activity = new ProgressActivity("act-1", DateTimeOffset.UtcNow, "Remotely synchronized activity");
        _activityClient.Activities.Add(activity);

        var request = new RefreshPulseRequest(sessionId);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(sessionId, result.Id);
        Assert.Equal(SessionStatus.InProgress, result.Pulse.Status);
        Assert.False(result.IsCached);
        Assert.True(_sessionWriter.Saved);
        
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
            .WithPulse(SessionStatus.InProgress, "Cached Progress")
            .Build();
            
        _sessionReader.Sessions[sessionId] = cachedSession;
        _pulseMonitor.ShouldThrow = true; 

        var request = new RefreshPulseRequest(sessionId);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(sessionId, result.Id);
        Assert.Equal(SessionStatus.InProgress, result.Pulse.Status);
        Assert.Equal("Cached Progress", result.Pulse.Detail);
        Assert.True(result.IsCached);
        Assert.NotNull(result.Warning);
    }

    [Fact(DisplayName = "Given a Handle that does not exist in the Task Registry, when refreshing the Pulse, then it should notify that the session is unknown.")]
    public async Task ShouldThrowWhenHandleNotFound()
    {
        // Arrange
        var sessionId = new SessionId("sessions/ghost-session");
        var request = new RefreshPulseRequest(sessionId);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ExecuteAsync(request, CancellationToken.None));
        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakePulseMonitor : IPulseMonitor
    {
        public bool ShouldThrow { get; set; }
        public Task<SessionPulse> GetSessionPulseAsync(SessionId id, CancellationToken cancellationToken = default)
        {
            if (ShouldThrow) throw new RemoteCollaboratorUnavailableException();
            return Task.FromResult(new SessionPulse(SessionStatus.InProgress, "All good!"));
        }

        public Task<Session> GetRemoteSessionAsync(SessionId id, TaskDescription originalTask, CancellationToken cancellationToken = default)
        {
            if (ShouldThrow) throw new RemoteCollaboratorUnavailableException();
            var session = new SessionBuilder()
                .WithId(id.Value)
                .WithTask((string)originalTask)
                .WithPulse(SessionStatus.InProgress, "All good!")
                .Build();
            return Task.FromResult(session);
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
