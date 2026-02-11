using Cleo.Core.Domain.Services;
using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.Services;

public class TimestampBasedPlanResolutionStrategyTests
{
    private readonly TimestampBasedPlanResolutionStrategy _strategy = new();

    [Fact(DisplayName = "ResolvePlan should return null if no planning activities exist.")]
    public void ResolvePlanShouldReturnNullIfEmpty()
    {
        var activities = new List<SessionActivity>
        {
            new ProgressActivity("act/1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "working...")
        };

        var result = _strategy.ResolvePlan(activities);

        Assert.Null(result);
    }

    [Fact(DisplayName = "ResolvePlan should return the single planning activity if only one exists.")]
    public void ResolvePlanShouldReturnSinglePlan()
    {
        var plan = new PlanningActivity("act/2", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "plan-1", Array.Empty<PlanStep>());
        var activities = new List<SessionActivity> { plan };

        var result = _strategy.ResolvePlan(activities);

        Assert.Same(plan, result);
    }

    [Fact(DisplayName = "ResolvePlan should return the latest planning activity by timestamp.")]
    public void ResolvePlanShouldReturnLatestByTimestamp()
    {
        var now = DateTimeOffset.UtcNow;
        var olderPlan = new PlanningActivity("act/old", now.AddHours(-1), ActivityOriginator.Agent, "old-plan", Array.Empty<PlanStep>());
        var newerPlan = new PlanningActivity("act/new", now, ActivityOriginator.Agent, "new-plan", Array.Empty<PlanStep>());

        var activities = new List<SessionActivity>
        {
            olderPlan,
            new ProgressActivity("act/p", now.AddMinutes(30), ActivityOriginator.Agent, "working..."),
            newerPlan
        };

        var result = _strategy.ResolvePlan(activities);

        Assert.Same(newerPlan, result);
    }

    [Fact(DisplayName = "ResolvePlan should throw ArgumentNullException if activities list is null.")]
    public void ResolvePlanShouldValidateInput()
    {
        Assert.Throws<ArgumentNullException>(() => _strategy.ResolvePlan(null!));
    }
}
