using Cleo.Core.Domain.Services;
using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.Services;

internal class TimestampBasedPlanResolutionStrategyTests
{
    private readonly TimestampBasedPlanResolutionStrategy _strategy = new();

    [Fact(DisplayName = "ResolvePlan should return null if no planning activities exist.")]
    public void ResolvePlanShouldReturnNullIfEmpty()
    {
        var activities = new List<SessionActivity>
        {
            new ProgressActivity("act/1", "r1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "working...")
        };

        var result = _strategy.ResolvePlan(activities);

        Assert.Null(result);
    }

    [Fact(DisplayName = "ResolvePlan should return the single planning activity if only one exists.")]
    public void ResolvePlanShouldReturnSinglePlan()
    {
        var plan = new PlanningActivity("act/2", "r2", DateTimeOffset.UtcNow, ActivityOriginator.Agent, new PlanId("plans/plan-1"), Array.Empty<PlanStep>());
        var activities = new List<SessionActivity> { plan };

        var result = _strategy.ResolvePlan(activities);

        Assert.Same(plan, result);
    }

    [Fact(DisplayName = "ResolvePlan should return the latest planning activity by timestamp.")]
    public void ResolvePlanShouldReturnLatestByTimestamp()
    {
        var now = DateTimeOffset.UtcNow;
        var olderPlan = new PlanningActivity("act/old", "r-old", now.AddHours(-1), ActivityOriginator.Agent, new PlanId("plans/old-plan"), Array.Empty<PlanStep>());
        var newerPlan = new PlanningActivity("act/new", "r-new", now, ActivityOriginator.Agent, new PlanId("plans/new-plan"), Array.Empty<PlanStep>());

        var activities = new List<SessionActivity>
        {
            olderPlan,
            new ProgressActivity("act/p", "r-p", now.AddMinutes(30), ActivityOriginator.Agent, "working..."),
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
