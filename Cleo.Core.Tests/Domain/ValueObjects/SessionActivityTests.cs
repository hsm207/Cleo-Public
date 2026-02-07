using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class SessionActivityTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    [Fact(DisplayName = "MessageActivity should store text and originator.")]
    public void MessageActivityTest()
    {
        var activity = new MessageActivity("id1", Now, ActivityOriginator.User, "Hello!");
        Assert.Equal("id1", activity.Id);
        Assert.Equal(Now, activity.Timestamp);
        Assert.Equal(ActivityOriginator.User, activity.Originator);
        Assert.Equal("Hello!", activity.Text);
    }

    [Fact(DisplayName = "PlanningActivity should store plan steps.")]
    public void PlanningActivityTest()
    {
        var steps = new[] { new PlanStep(0, "T1", "D1") };
        var activity = new PlanningActivity("id2", Now, steps);
        Assert.Equal(ActivityOriginator.Agent, activity.Originator);
        Assert.Single(activity.Steps);
        Assert.Equal("T1", activity.Steps.First().Title);
    }

    [Fact(DisplayName = "ExecutionActivity should store command details.")]
    public void ExecutionActivityTest()
    {
        var activity = new ExecutionActivity("id3", Now, "ls", "out", 0);
        Assert.Equal("ls", activity.Command);
        Assert.Equal("out", activity.Output);
        Assert.Equal(0, activity.ExitCode);
    }

    [Fact(DisplayName = "ProgressActivity should store detail heartbeat.")]
    public void ProgressActivityTest()
    {
        var activity = new ProgressActivity("id4", Now, "Still working...");
        Assert.Equal("Still working...", activity.Detail);
    }

    [Fact(DisplayName = "ResultActivity should store the solution patch.")]
    public void ResultActivityTest()
    {
        var patch = new SolutionPatch("diff", "base");
        var activity = new ResultActivity("id5", Now, patch);
        Assert.Equal(patch, activity.Patch);
    }

    [Fact(DisplayName = "FailureActivity should store the reason.")]
    public void FailureActivityTest()
    {
        var activity = new FailureActivity("id6", Now, "Quota exceeded");
        Assert.Equal("Quota exceeded", activity.Reason);
        Assert.Equal(ActivityOriginator.System, activity.Originator);
    }
}
