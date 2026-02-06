using Cleo.Core.Domain.Common;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Events;

/// <summary>
/// Fired when a new task is successfully assigned to a remote Jules session.
/// </summary>
public record SessionAssigned(
    SessionId SessionId, 
    TaskDescription Task, 
    DateTimeOffset OccurredOn) : IDomainEvent
{
    public SessionAssigned(SessionId sessionId, TaskDescription task) 
        : this(sessionId, task, DateTimeOffset.UtcNow) { }
}

/// <summary>
/// Fired when a new heartbeat pulse is received from Jules.
/// </summary>
public record StatusHeartbeatReceived(
    SessionId SessionId, 
    SessionPulse Pulse, 
    DateTimeOffset OccurredOn) : IDomainEvent
{
    public StatusHeartbeatReceived(SessionId sessionId, SessionPulse pulse) 
        : this(sessionId, pulse, DateTimeOffset.UtcNow) { }
}

/// <summary>
/// Fired when Jules is paused and waiting for developer feedback.
/// </summary>
public record FeedbackRequested(
    SessionId SessionId, 
    string? Prompt, 
    DateTimeOffset OccurredOn) : IDomainEvent
{
    public FeedbackRequested(SessionId sessionId, string? prompt) 
        : this(sessionId, prompt, DateTimeOffset.UtcNow) { }
}

/// <summary>
/// Fired when a code solution (patch) is ready for pull.
/// </summary>
public record SolutionReady(
    SessionId SessionId, 
    SolutionPatch Solution, 
    DateTimeOffset OccurredOn) : IDomainEvent
{
    public SolutionReady(SessionId sessionId, SolutionPatch solution) 
        : this(sessionId, solution, DateTimeOffset.UtcNow) { }
}
