using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class NarrativeIntelligenceTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;
    private const string RemoteId = "remote-id";

    [Fact(DisplayName = "ProgressActivity with Thought should be Significant (Reasoning Signal).")]
    public void ReasoningSignalIsSignificant()
    {
        var activity = new ProgressActivity("id", RemoteId, Now, ActivityOriginator.Agent, "Thinking...", "I need to check the files.");

        Assert.True(activity.IsSignificant, "Activities with internal monologue must be significant.");
        Assert.Equal("Thinking...", activity.Intent);
        Assert.Equal("I need to check the files.", activity.Reasoning); // Updated to Reasoning

        // The summary should contain the intent.
        var summary = activity.GetContentSummary();
        Assert.Equal("Thinking...", summary);
    }

    [Fact(DisplayName = "ProgressActivity with Artifacts should be Significant (Outcome Signal).")]
    public void OutcomeSignalIsSignificant()
    {
        var evidence = new List<Artifact> { new BashOutput("ls", "files", 0) };
        var activity = new ProgressActivity("id", RemoteId, Now, ActivityOriginator.Agent, "Done", null, evidence);

        Assert.True(activity.IsSignificant, "Activities with artifacts must be significant.");
        Assert.Equal("Done", activity.GetContentSummary());
    }

    [Fact(DisplayName = "ProgressActivity with ONLY Title should be Insignificant (Trace Signal).")]
    public void TraceSignalIsInsignificant()
    {
        var activity = new ProgressActivity("id", RemoteId, Now, ActivityOriginator.Agent, "Just checking in...");

        Assert.False(activity.IsSignificant, "Pure heartbeats without thoughts or artifacts should be insignificant.");
        Assert.Equal("Just checking in...", activity.GetContentSummary());
    }

    [Fact(DisplayName = "ChangeSet should summarize huge file lists (Signal-to-Noise).")]
    public void ChangeSetSummarizesImpactMagnitude()
    {
        var files = Enumerable.Range(0, 50).Select(i => $"Cleo.Core/Domain/Entity{i}.cs").ToList();
        var diff = string.Join("\n", files.Select(f => $"+++ b/{f}"));

        var patch = new GitPatch(diff, "base-sha");
        var changeSet = new ChangeSet("repo", patch);

        var summary = changeSet.GetSummary();

        Assert.Contains("50 Cleo.Core/Domain/* modified", summary, StringComparison.Ordinal);
        Assert.DoesNotContain("Entity0.cs", summary, StringComparison.Ordinal); // Should hide the wall of text
    }

    [Fact(DisplayName = "ChangeSet should list files for small changes (Audit Escape Hatch).")]
    public void ChangeSetListsSmallChanges()
    {
        var diff = "+++ b/File1.cs\n+++ b/File2.cs";
        var patch = new GitPatch(diff, "base-sha");
        var changeSet = new ChangeSet("repo", patch);

        var summary = changeSet.GetSummary();

        Assert.Contains("Updated [File1.cs, File2.cs]", summary, StringComparison.Ordinal);
    }
}
