using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Policies;

/// <summary>
/// Defines the policy for evaluating the high-level Session State from low-level signals.
/// </summary>
public interface IPulseEvaluationPolicy
{
    /// <summary>
    /// Evaluates the session state based on the current pulse, history, and delivery status.
    /// </summary>
    /// <param name="pulse">The current session pulse (physical state).</param>
    /// <param name="history">The full session history.</param>
    /// <param name="isDelivered">Whether the session has delivered a solution (PR or ChangeSet).</param>
    /// <returns>The evaluated Session State.</returns>
    SessionState Evaluate(SessionPulse pulse, IEnumerable<SessionActivity> history, bool isDelivered);
}
