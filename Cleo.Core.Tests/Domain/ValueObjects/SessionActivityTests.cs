using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class SessionActivityTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;
    private const string RemoteId = "remote-id";

    [Fact(DisplayName = "ProgressActivity should format Intent, Multiline Thought, and Evidence correctly.")]
    public void ProgressActivityFormattingScenario()
    {
        // ... (Same as before)
        var intent = "Refactoring";
        var thought = "Line 1\nLine 2";
        var output = new BashOutput("echo", "hi", 0);
        var snapshot = new MediaArtifact("img/png", "data");
        
        var activity = new ProgressActivity("id", RemoteId, Now, ActivityOriginator.Agent, intent, thought, new Artifact[] { output, snapshot });

        var summary = activity.GetContentSummary();

        Assert.StartsWith(intent, summary, StringComparison.Ordinal);
        Assert.Contains("💭 Line 1", summary, StringComparison.Ordinal);
        Assert.Contains("             Line 2", summary, StringComparison.Ordinal);
        Assert.Contains("📦 " + output.GetSummary(), summary, StringComparison.Ordinal);
        Assert.Contains("📦 " + snapshot.GetSummary(), summary, StringComparison.Ordinal);
        Assert.True(activity.IsSignificant);
    }

    [Fact(DisplayName = "ProgressActivity should handle minimal state (Intent only).")]
    public void ProgressActivityMinimalScenario()
    {
        var activity = new ProgressActivity("id", RemoteId, Now, ActivityOriginator.Agent, "Just Chilling");
        Assert.Equal("Just Chilling", activity.GetContentSummary());
        Assert.False(activity.IsSignificant);
    }

    [Fact(DisplayName = "CompletionActivity should format success message with optional artifacts.")]
    public void CompletionActivityFormatting()
    {
        var simple = new CompletionActivity("id", RemoteId, Now, ActivityOriginator.System);
        Assert.Equal("Session Completed Successfully", simple.GetContentSummary());

        var patch = new GitPatch("diff", "sha");
        var changeSet = new ChangeSet("repo", patch);
        var complex = new CompletionActivity("id", RemoteId, Now, ActivityOriginator.System, new[] { changeSet });
        
        Assert.Contains("Session Completed Successfully | ", complex.GetContentSummary(), StringComparison.Ordinal);
        Assert.Contains(changeSet.GetSummary(), complex.GetContentSummary(), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "ApprovalActivity should display plan ID.")]
    public void ApprovalActivityFormatting()
    {
        var activity = new ApprovalActivity("id", RemoteId, Now, ActivityOriginator.User, "plan-123");
        Assert.Equal("Plan Approved: plan-123", activity.GetContentSummary());
    }

    [Fact(DisplayName = "PlanningActivity should display step count and plan ID.")]
    public void PlanningActivityFormatting()
    {
        var steps = new[] { new PlanStep("s1", 1, "T", "D") };
        var activity = new PlanningActivity("id", RemoteId, Now, ActivityOriginator.Agent, "plan-1", steps);

        Assert.Equal("Plan Generated: plan-1 (1 steps)", activity.GetContentSummary());
        Assert.Contains("Steps: 1", activity.GetMetaDetail(), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "FailureActivity should display reason.")]
    public void FailureActivityFormatting()
    {
        var activity = new FailureActivity("id", RemoteId, Now, ActivityOriginator.System, "Crashed");
        Assert.Equal("FAILURE: Crashed", activity.GetContentSummary());
    }

    [Fact(DisplayName = "MessageActivity should display text.")]
    public void MessageActivityFormatting()
    {
        var activity = new MessageActivity("id", RemoteId, Now, ActivityOriginator.User, "Hello");
        Assert.Equal("Hello", activity.GetContentSummary());
    }

    [Fact(DisplayName = "SessionActivity should enforce structural equality (Value Object semantics).")]
    public void SessionActivityValueSemantics()
    {
        var act1 = new MessageActivity("id", "rem", Now, ActivityOriginator.User, "Hi");
        var act2 = new MessageActivity("id", "rem", Now, ActivityOriginator.User, "Hi");
        var act3 = new MessageActivity("id", "rem", Now, ActivityOriginator.User, "Bye"); // Diff text

        Assert.Equal(act1, act2); // Value Equality
        Assert.NotEqual(act1, act3);
        Assert.Equal(act1.GetHashCode(), act2.GetHashCode());
        
        // Cover ToString() for the record hierarchy
        Assert.Contains("MessageActivity", act1.ToString(), StringComparison.Ordinal);
        Assert.Contains("Hi", act1.ToString(), StringComparison.Ordinal);
    }
}
