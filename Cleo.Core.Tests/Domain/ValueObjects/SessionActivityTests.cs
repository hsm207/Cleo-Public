using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class SessionActivityTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;
    private const string RemoteId = "remote-id";

    [Fact(DisplayName = "ProgressActivity should show Intent.")]
    public void ProgressActivityShouldShowIntent()
    {
        var activity = new ProgressActivity("id", RemoteId, Now, ActivityOriginator.Agent, "Working hard!");
        Assert.Equal("Working hard!", activity.GetContentSummary());
        Assert.Equal("Working hard!", activity.Intent);
        Assert.Null(activity.Thought);
        Assert.False(activity.IsSignificant); // No thought, no evidence -> Not significant
    }

    [Fact(DisplayName = "ProgressActivity should show Thought when present.")]
    public void ProgressActivityShouldShowThought()
    {
        var activity = new ProgressActivity("id", RemoteId, Now, ActivityOriginator.Agent, "Working", "I am thinking...");

        Assert.Equal("Working", activity.Intent);
        Assert.Equal("I am thinking...", activity.Thought);
        Assert.Contains("💭 I am thinking...", activity.GetContentSummary(), StringComparison.Ordinal);
        Assert.True(activity.IsSignificant); // Has thought -> Significant
    }

    [Fact(DisplayName = "ProgressActivity should summarize single artifact when Detail is empty.")]
    public void ProgressActivityShouldSummarizeSingleArtifact()
    {
        var patch = new GitPatch("diff", "sha");
        var changeSet = new ChangeSet("repo", patch);
        var activity = new ProgressActivity("id", RemoteId, Now, ActivityOriginator.Agent, "", null, new[] { changeSet });

        Assert.Contains("📦 " + changeSet.GetSummary(), activity.GetContentSummary(), StringComparison.Ordinal);
        Assert.True(activity.IsSignificant); // Has evidence -> Significant
    }

    [Fact(DisplayName = "ProgressActivity should summarize multiple artifacts when Detail is empty.")]
    public void ProgressActivityShouldSummarizeMultipleArtifacts()
    {
        var output = new BashOutput("echo", "hi", 0);
        var snapshot = new MediaArtifact("img/png", "data");
        var activity = new ProgressActivity("id", RemoteId, Now, ActivityOriginator.Agent, "", null, new Artifact[] { output, snapshot });

        var summary = activity.GetContentSummary();
        Assert.Contains("📦 " + output.GetSummary(), summary, StringComparison.Ordinal);
        Assert.Contains("📦 " + snapshot.GetSummary(), summary, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "CompletionActivity should show completion message when no artifacts.")]
    public void CompletionActivityShouldShowDefault()
    {
        var activity = new CompletionActivity("id", RemoteId, Now, ActivityOriginator.System);
        Assert.Equal("Session Completed Successfully", activity.GetContentSummary());
        Assert.True(activity.IsSignificant);
    }

    [Fact(DisplayName = "CompletionActivity should append artifacts to completion message.")]
    public void CompletionActivityShouldAppendArtifacts()
    {
        var patch = new GitPatch("diff", "sha");
        var changeSet = new ChangeSet("repo", patch);
        var activity = new CompletionActivity("id", RemoteId, Now, ActivityOriginator.System, new[] { changeSet });

        Assert.Equal($"Session Completed Successfully | {changeSet.GetSummary()}", activity.GetContentSummary());
    }

    [Fact(DisplayName = "ApprovalActivity should show PlanId.")]
    public void ApprovalActivityShouldShowPlanId()
    {
        var activity = new ApprovalActivity("id", RemoteId, Now, ActivityOriginator.User, "plan-123");
        Assert.Equal("Plan Approved: plan-123", activity.GetContentSummary());
        Assert.Equal("plan-123", activity.PlanId);
        Assert.True(activity.IsSignificant);
    }

    [Fact(DisplayName = "PlanningActivity should show steps count.")]
    public void PlanningActivityShouldShowSteps()
    {
        var steps = new[] { new PlanStep("s1", 1, "T", "D") };
        var activity = new PlanningActivity("id", RemoteId, Now, ActivityOriginator.Agent, "plan-1", steps);

        Assert.Equal("Plan Generated: plan-1 (1 steps)", activity.GetContentSummary());
        Assert.Contains("Steps: 1", activity.GetMetaDetail(), StringComparison.Ordinal);
        Assert.Equal("plan-1", activity.PlanId);
        Assert.Equal(steps, activity.Steps);
        Assert.True(activity.IsSignificant);
    }

    [Fact(DisplayName = "FailureActivity should show reason.")]
    public void FailureActivityShouldShowReason()
    {
        var activity = new FailureActivity("id", RemoteId, Now, ActivityOriginator.System, "Crashed");
        Assert.Equal("FAILURE: Crashed", activity.GetContentSummary());
        Assert.Equal("Crashed", activity.Reason);
        Assert.True(activity.IsSignificant);
    }

    [Fact(DisplayName = "MessageActivity should show text.")]
    public void MessageActivityShouldShowText()
    {
        var activity = new MessageActivity("id", RemoteId, Now, ActivityOriginator.User, "Hello");
        Assert.Equal("Hello", activity.GetContentSummary());
        Assert.Equal("Hello", activity.Text);
        Assert.True(activity.IsSignificant);
    }

    [Fact(DisplayName = "SessionActivity MetaDetail should include generic info.")]
    public void MetaDetailShouldIncludeGenericInfo()
    {
        var activity = new MessageActivity("id", RemoteId, Now, ActivityOriginator.User, "Hello");
        var meta = activity.GetMetaDetail();
        Assert.Contains($"Originator: {ActivityOriginator.User}", meta, StringComparison.Ordinal);
        Assert.Contains("Evidence: 0", meta, StringComparison.Ordinal);
    }
}
