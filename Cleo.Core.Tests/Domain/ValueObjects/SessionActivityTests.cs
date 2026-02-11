using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class SessionActivityTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    [Fact(DisplayName = "ProgressActivity should show Detail when present.")]
    public void ProgressActivityShouldShowDetail()
    {
        var activity = new ProgressActivity("id", Now, ActivityOriginator.Agent, "Working hard!");
        Assert.Equal("Working hard!", activity.GetContentSummary());
    }

    [Fact(DisplayName = "ProgressActivity should summarize single artifact when Detail is empty.")]
    public void ProgressActivityShouldSummarizeSingleArtifact()
    {
        var patch = new GitPatch("diff", "sha");
        var changeSet = new ChangeSet("repo", patch);
        var activity = new ProgressActivity("id", Now, ActivityOriginator.Agent, "", null, new[] { changeSet });

        Assert.Equal("\n          📦 " + changeSet.GetSummary(), activity.GetContentSummary());
    }

    [Fact(DisplayName = "ProgressActivity should summarize multiple artifacts when Detail is empty.")]
    public void ProgressActivityShouldSummarizeMultipleArtifacts()
    {
        var output = new BashOutput("echo", "hi", 0);
        var snapshot = new VisualSnapshot("img/png", "data");
        var activity = new ProgressActivity("id", Now, ActivityOriginator.Agent, "", null, new Artifact[] { output, snapshot });

        Assert.Equal("\n          📦 " + output.GetSummary() + "\n          📦 " + snapshot.GetSummary(), activity.GetContentSummary());
    }

    [Fact(DisplayName = "CompletionActivity should show completion message when no artifacts.")]
    public void CompletionActivityShouldShowDefault()
    {
        var activity = new CompletionActivity("id", Now, ActivityOriginator.System);
        Assert.Equal("Session Completed Successfully", activity.GetContentSummary());
    }

    [Fact(DisplayName = "CompletionActivity should append artifacts to completion message.")]
    public void CompletionActivityShouldAppendArtifacts()
    {
        var patch = new GitPatch("diff", "sha");
        var changeSet = new ChangeSet("repo", patch);
        var activity = new CompletionActivity("id", Now, ActivityOriginator.System, new[] { changeSet });

        Assert.Equal($"Session Completed Successfully | {changeSet.GetSummary()}", activity.GetContentSummary());
    }

    [Fact(DisplayName = "ProgressActivity should be marked as Low significance.")]
    public void ProgressActivityShouldBeLowSignificance()
    {
        var activity = new ProgressActivity("id", Now, ActivityOriginator.Agent, "Working");
        Assert.False(activity.IsSignificant);
    }

    [Theory(DisplayName = "Significant activities should be marked as High significance.")]
    [MemberData(nameof(SignificantActivities))]
    public void SignificantActivitiesShouldBeHighSignificance(SessionActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);
        Assert.True(activity.IsSignificant);
    }

    public static TheoryData<SessionActivity> SignificantActivities => new()
    {
        new PlanningActivity("id", Now, ActivityOriginator.Agent, "planId", new List<PlanStep>()),
        new MessageActivity("id", Now, ActivityOriginator.User, "hello"),
        new ApprovalActivity("id", Now, ActivityOriginator.User, "planId"),
        new CompletionActivity("id", Now, ActivityOriginator.System),
        new FailureActivity("id", Now, ActivityOriginator.System, "reason")
    };
}
