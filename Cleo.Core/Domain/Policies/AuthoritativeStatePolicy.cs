using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Policies;

/// <summary>
/// Encapsulates the rules for logical session state overrides.
/// </summary>
public class AuthoritativeStatePolicy : IPulseEvaluationPolicy
{
    public SessionState Evaluate(SessionPulse pulse, IEnumerable<SessionActivity> history, bool isDelivered)
    {
        ArgumentNullException.ThrowIfNull(pulse);
        ArgumentNullException.ThrowIfNull(history);

        var physicalState = MapToState(pulse.Status);

        // Logical State Override: If Idle + No PR + Last Activity was Planning -> AwaitingPlanApproval
        if (physicalState == SessionState.Idle && !isDelivered)
        {
            var lastActivity = history
                .OrderByDescending(a => a.Timestamp)
                .FirstOrDefault(a => a.IsSignificant);

            if (lastActivity is PlanningActivity)
            {
                return SessionState.AwaitingPlanApproval;
            }
        }

        return physicalState;
    }

    private static SessionState MapToState(SessionStatus status) => status switch
    {
        SessionStatus.StartingUp => SessionState.Queued,
        SessionStatus.Planning => SessionState.Planning,
        SessionStatus.InProgress => SessionState.Working,
        SessionStatus.Paused => SessionState.Paused, // Paused is now a distinct state 🛑
        SessionStatus.AwaitingFeedback => SessionState.AwaitingFeedback,
        SessionStatus.AwaitingPlanApproval => SessionState.AwaitingPlanApproval,
        SessionStatus.Completed => SessionState.Idle,
        SessionStatus.Abandoned => SessionState.Idle,
        SessionStatus.Failed => SessionState.Broken,
        // StateUnspecified or unknown values map to WTF 🚨
        _ => SessionState.WTF
    };
}
