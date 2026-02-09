using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class SessionActivityTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    [Fact(DisplayName = "MessageActivity should store text, originator, and evidence.")]
    public void MessageActivityTest()
    {
        var evidence = new List<Artifact> { new CommandEvidence("ls", "out", 0) };
        var activity = new MessageActivity("id1", Now, ActivityOriginator.User, "Hello!", evidence);
        Assert.Equal("id1", activity.Id);
        Assert.Equal(Now, activity.Timestamp);
        Assert.Equal(ActivityOriginator.User, activity.Originator);
        Assert.Equal("Hello!", activity.Text);
        Assert.Equal(evidence, activity.Evidence);
    }

    [Fact(DisplayName = "PlanningActivity should store plan steps and evidence.")]
    public void PlanningActivityTest()
    {
        var steps = new[] { new PlanStep(0, "T1", "D1") };
        var evidence = new List<Artifact> { new MediaEvidence("mime", "data") };
        var activity = new PlanningActivity("id2", Now, "plan-1", steps, evidence);
        Assert.Equal(ActivityOriginator.Agent, activity.Originator);
        Assert.Single(activity.Steps);
        Assert.Equal("plan-1", activity.PlanId);
        Assert.Equal("T1", activity.Steps.First().Title);
        Assert.Equal(evidence, activity.Evidence);
    }

    [Fact(DisplayName = "ApprovalActivity should store plan identifier.")]
    public void ApprovalActivityTest()
    {
        var activity = new ApprovalActivity("id3", Now, "plan-123");
        Assert.Equal("plan-123", activity.PlanId);
        Assert.Equal(ActivityOriginator.User, activity.Originator);
    }

    [Fact(DisplayName = "Artifact types should store their respective data.")]
    public void ArtifactTypesTest()
    {
        var cmd = new CommandEvidence("ls", "out", 0);
        var patch = new CodeProposal(new SolutionPatch("d", "b"));
        var media = new MediaEvidence("m", "d");

        Assert.Equal("ls", cmd.Command);
        Assert.Equal("d", patch.Patch.UniDiff);
        Assert.Equal("m", media.MimeType);
    }

    [Fact(DisplayName = "ProgressActivity should store detail heartbeat.")]
    public void ProgressActivityTest()
    {
        var activity = new ProgressActivity("id4", Now, "Still working...");
        Assert.Equal("Still working...", activity.Detail);
    }

    [Fact(DisplayName = "FailureActivity should store the reason.")]
    public void FailureActivityTest()
    {
        var activity = new FailureActivity("id6", Now, "Quota exceeded");
        Assert.Equal("Quota exceeded", activity.Reason);
        Assert.Equal(ActivityOriginator.System, activity.Originator);
    }
}
