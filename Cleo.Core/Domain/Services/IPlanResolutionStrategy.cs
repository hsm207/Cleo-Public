using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Services;

/// <summary>
/// A strategy for resolving the authoritative plan from a collection of session activities.
/// This allows for different policies (e.g., Timestamp-based, Approved-only) to be plugged in.
/// </summary>
public interface IPlanResolutionStrategy
{
    /// <summary>
    /// Selects the authoritative plan from the given history.
    /// </summary>
    PlanningActivity? ResolvePlan(IEnumerable<SessionActivity> activities);
}
