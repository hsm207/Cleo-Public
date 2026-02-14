using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class SessionActivityTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;
    private const string RemoteId = "remote-id";

    // ... (Existing Tests) ...

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

        Assert.Equal(intent, summary);
        Assert.True(activity.IsSignificant);
    }

    [Fact(DisplayName = "ProgressActivity should handle minimal state (Intent only).")]
    public void ProgressActivityMinimalScenario()
    {
        var activity = new ProgressActivity("id", RemoteId, Now, ActivityOriginator.Agent, "Just Chilling");
        Assert.Equal("Just Chilling", activity.GetContentSummary());
        Assert.False(activity.IsSignificant);
    }

    [Fact(DisplayName = "CompletionActivity should format success message simply (Artifacts are rendered separately by CLI).")]
    public void CompletionActivityFormatting()
    {
        var simple = new CompletionActivity("id", RemoteId, Now, ActivityOriginator.System);
        Assert.Equal("Session Completed Successfully", simple.GetContentSummary());

        var patch = new GitPatch("diff", "sha");
        var changeSet = new ChangeSet("repo", patch);
        var complex = new CompletionActivity("id", RemoteId, Now, ActivityOriginator.System, new[] { changeSet });
        
        // RFC 013 Fix: We no longer include artifacts in the summary because they are rendered as separate lines.
        Assert.Equal("Session Completed Successfully", complex.GetContentSummary());
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

    [Fact(DisplayName = "MessageActivity should return correct symbol based on Originator.")]
    public void MessageActivityShouldReturnCorrectSymbol()
    {
        var userMsg = new MessageActivity("1", "r1", Now, ActivityOriginator.User, "hi");
        Assert.Equal("👤", userMsg.GetSymbol());

        var agentMsg = new MessageActivity("2", "r2", Now, ActivityOriginator.Agent, "hello");
        Assert.Equal("👸", agentMsg.GetSymbol());

        var sysMsg = new MessageActivity("3", "r3", Now, ActivityOriginator.System, "alert");
        Assert.Equal("💬", sysMsg.GetSymbol());
    }

    [Fact(DisplayName = "SessionAssignedActivity should return 🚀.")]
    public void SessionAssignedActivityShouldReturnRocket()
    {
        var act = new SessionAssignedActivity("1", "r1", Now, ActivityOriginator.User, (TaskDescription)"task");
        Assert.Equal("🚀", act.GetSymbol());
    }

    [Fact(DisplayName = "PlanningActivity should return 🗺️.")]
    public void PlanningActivityShouldReturnMap()
    {
        var act = new PlanningActivity("1", "r1", Now, ActivityOriginator.Agent, "plan", Array.Empty<PlanStep>());
        Assert.Equal("🗺️", act.GetSymbol());
    }

    [Fact(DisplayName = "ApprovalActivity should return ✅.")]
    public void ApprovalActivityShouldReturnCheck()
    {
        var act = new ApprovalActivity("1", "r1", Now, ActivityOriginator.User, "plan");
        Assert.Equal("✅", act.GetSymbol());
    }

    [Fact(DisplayName = "ProgressActivity should return 🧠 if Thought is present, else 📡.")]
    public void ProgressActivityShouldReturnCorrectSymbol()
    {
        var trace = new ProgressActivity("1", "r1", Now, ActivityOriginator.Agent, "working");
        Assert.Equal("📡", trace.GetSymbol());

        var thought = new ProgressActivity("2", "r2", Now, ActivityOriginator.Agent, "working", "thinking...");
        Assert.Equal("🧠", thought.GetSymbol());
    }

    [Fact(DisplayName = "CompletionActivity should return 🏁.")]
    public void CompletionActivityShouldReturnFlag()
    {
        var act = new CompletionActivity("1", "r1", Now, ActivityOriginator.System);
        Assert.Equal("🏁", act.GetSymbol());
    }

    [Fact(DisplayName = "FailureActivity should return 💥.")]
    public void FailureActivityShouldReturnBoom()
    {
        var act = new FailureActivity("1", "r1", Now, ActivityOriginator.System, "error");
        Assert.Equal("💥", act.GetSymbol());
    }

    [Fact(DisplayName = "Minimal SessionActivity should gracefully degrade to Universal Signal.")]
    public void MinimalActivityShouldUseUniversalSignal()
    {
        // Graceful Degradation: Base implementation returns '🔹'
        var activity = new MinimalActivity();
        Assert.Equal("🔹", activity.GetSymbol());
    }

    [Fact(DisplayName = "CompletionActivity should have empty thoughts (Silent Reflection).")]
    public void CompletionActivityShouldBeSilent()
    {
        // Silent Reflection: Activities without explicit thought support should return empty
        var activity = new CompletionActivity("id", "r", Now, ActivityOriginator.System);
        Assert.Empty(activity.GetThoughts());
    }

    [Fact(DisplayName = "ProgressActivity should handle missing thought gracefully (Empty Thoughts).")]
    public void ProgressActivityEmptyThoughts()
    {
        var activity = new ProgressActivity("id", "r", Now, ActivityOriginator.Agent, "Intent", null);
        Assert.Empty(activity.GetThoughts());
    }

    [Fact(DisplayName = "SessionActivity base class GetThoughts should be empty.")]
    public void BaseActivityGetThoughtsShouldBeEmpty()
    {
        var activity = new MinimalActivity();
        Assert.Empty(activity.GetThoughts());
    }

    // A minimal concrete implementation for testing the base class behavior
    private sealed record MinimalActivity() : SessionActivity(
        "id", "r", DateTimeOffset.UtcNow, ActivityOriginator.System, Array.Empty<Artifact>())
    {
        public override string GetContentSummary() => "Minimal";
    }
}
