namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// Represents the monitorable heartbeat of a Jules session.
/// </summary>
public enum SessionStatus
{
    StartingUp,
    Planning,
    InProgress,
    AwaitingFeedback,
    Completed,
    Failed
}

public record SessionPulse(SessionStatus Status, string? Detail = null);
