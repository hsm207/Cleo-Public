using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Default implementation for mapping Jules API states to domain statuses.
/// </summary>
internal sealed class DefaultSessionStatusMapper : ISessionStatusMapper
{
    private static readonly Dictionary<JulesSessionState, SessionStatus> StatusMap = new()
    {
        [JulesSessionState.StateUnspecified] = SessionStatus.StateUnspecified,
        [JulesSessionState.StartingUp] = SessionStatus.StartingUp,
        [JulesSessionState.Queued] = SessionStatus.StartingUp,
        [JulesSessionState.Planning] = SessionStatus.Planning,
        [JulesSessionState.InProgress] = SessionStatus.InProgress,
        [JulesSessionState.AwaitingUserFeedback] = SessionStatus.AwaitingFeedback,
        [JulesSessionState.AwaitingPlanApproval] = SessionStatus.AwaitingPlanApproval,
        [JulesSessionState.Paused] = SessionStatus.Paused,
        [JulesSessionState.Completed] = SessionStatus.Completed,
        [JulesSessionState.Failed] = SessionStatus.Failed
    };

    public SessionStatus Map(JulesSessionState? state) => 
        (state.HasValue && StatusMap.TryGetValue(state.Value, out var status)) ? status : SessionStatus.StateUnspecified;
}
