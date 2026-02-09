namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// Represents the physical 'Pose' of the remote collaborator.
/// </summary>
public enum Stance
{
    WTF,
    Queued,
    Planning,
    AwaitingPlanApproval,
    AwaitingFeedback,
    Working,
    Interrupted,
    Broken,
    Idle
}
