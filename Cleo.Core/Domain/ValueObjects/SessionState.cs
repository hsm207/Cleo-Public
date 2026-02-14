namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// The user-facing state of the session (formerly Stance).
/// Answers "What is the session's current posture?" 🧘‍♀️
/// </summary>
#pragma warning disable CA1724 // Conflict with System.Web.SessionState
public enum SessionState
{
    Unknown,
    Queued,
    Planning,
    AwaitingPlanApproval,
    AwaitingFeedback,
    Working,
    Paused,
    Interrupted,
    Broken,
    Idle
}
