namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// Represents the monitorable heartbeat of a Jules session.
/// </summary>
public enum SessionStatus
{
    StateUnspecified, // 0 in protobuf
    StartingUp,
    Planning,
    InProgress,
    Paused,
    AwaitingFeedback,
    AwaitingPlanApproval,
    Completed,
    Abandoned,
    Failed
}

public record SessionPulse(SessionStatus Status);
