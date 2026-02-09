using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Default implementation for mapping Jules API states to domain statuses.
/// </summary>
internal sealed class DefaultSessionStatusMapper : ISessionStatusMapper
{
    private static readonly Dictionary<string, SessionStatus> StatusMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["QUEUED"] = SessionStatus.StartingUp,
        ["STARTING_UP"] = SessionStatus.StartingUp,
        ["PLANNING"] = SessionStatus.Planning,
        ["IN_PROGRESS"] = SessionStatus.InProgress,
        ["AWAITING_USER_FEEDBACK"] = SessionStatus.AwaitingFeedback,
        ["AWAITING_PLAN_APPROVAL"] = SessionStatus.AwaitingPlanApproval,
        ["PAUSED"] = SessionStatus.InProgress,
        ["COMPLETED"] = SessionStatus.Completed,
        ["FAILED"] = SessionStatus.Failed
    };

    public SessionStatus Map(string? state) => 
        (state != null && StatusMap.TryGetValue(state, out var status)) ? status : SessionStatus.InProgress;
}
