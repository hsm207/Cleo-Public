using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Services;

/// <summary>
/// The standard RFC003 implementation of plan resolution:
/// "Scan the SessionLog for all activities of type PlanningActivity. Identify the activity with the most recent Timestamp."
/// </summary>
public class TimestampBasedPlanResolutionStrategy : IPlanResolutionStrategy
{
    public PlanningActivity? ResolvePlan(IEnumerable<SessionActivity> activities)
    {
        ArgumentNullException.ThrowIfNull(activities);

        return activities
            .OfType<PlanningActivity>()
            .OrderByDescending(a => a.Timestamp)
            .FirstOrDefault();
    }
}
