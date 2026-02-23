using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public sealed class ChangeSetTests
{
    private static readonly GitPatch Patch = GitPatch.FromApi("diff", "sha");

    [Fact(DisplayName = "ChangeSet should be created with valid arguments.")]
    public void ShouldCreateWithValidArgs()
    {
        var changeSet = new ChangeSet("sources/repo", Patch);

        Assert.Equal("sources/repo", changeSet.Source);
        Assert.Equal(Patch, changeSet.Patch);
    }

    [Fact(DisplayName = "ChangeSet should throw if Source is empty.")]
    public void ShouldThrowIfSourceEmpty()
    {
        Assert.Throws<ArgumentException>(() => new ChangeSet("", Patch));
        Assert.Throws<ArgumentException>(() => new ChangeSet(" ", Patch));
        Assert.Throws<ArgumentNullException>(() => new ChangeSet(null!, Patch));
    }

    [Fact(DisplayName = "ChangeSet should throw if Patch is null.")]
    public void ShouldThrowIfPatchNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ChangeSet("source", null!));
    }

    [Fact(DisplayName = "ChangeSet should provide a human-friendly summary with file details, short SHA, and fingerprint.")]
    public void ShouldProvideSummary()
    {
        var diff = @"--- a/file1.cs
+++ b/file1.cs
--- a/README.md
+++ b/README.md
";
        var patch = GitPatch.FromApi(diff, "852ae2160ccaefa8112af65941560654ad32261c");
        var changeSet = new ChangeSet("sources/github/hsm207/Cleo", patch);

        var expectedFingerprint = patch.Fingerprint[..7];
        var expectedSha = "852ae21";

        Assert.Equal($"ChangeSet [{expectedSha}:{expectedFingerprint}]: Updated [file1.cs, README.md]", changeSet.GetSummary());
    }

    [Fact(DisplayName = "ChangeSet should summarize impact magnitude when files exceed narrative threshold.")]
    public void ShouldSummarizeImpactMagnitude()
    {
        // 6 files under same directory
        var diff = "";
        for (int i = 0; i < 6; i++)
        {
            diff += $"+++ b/src/Common/{i}.cs\n";
        }

        var patch = GitPatch.FromApi(diff, "sha1234");
        var changeSet = new ChangeSet("source", patch);
        var fp = patch.Fingerprint[..7];

        // Common path should be "src/Common"
        var summary = changeSet.GetSummary();
        Assert.Equal($"ChangeSet [sha1234:{fp}]: 6 src/Common/* modified", summary);
    }

    [Fact(DisplayName = "ChangeSet should handle no common path when summarizing impact magnitude.")]
    public void ShouldSummarizeNoCommonPath()
    {
        // 6 files with no common root
        var diff = "";
        for (int i = 0; i < 6; i++)
        {
            diff += $"+++ b/{i}.cs\n";
        }

        var patch = GitPatch.FromApi(diff, "sha1234");
        var changeSet = new ChangeSet("source", patch);
        var fp = patch.Fingerprint[..7];

        var summary = changeSet.GetSummary();
        Assert.Equal($"ChangeSet [sha1234:{fp}]: 6 files modified", summary);
    }

    [Fact(DisplayName = "ChangeSet should handle empty file list.")]
    public void ShouldHandleEmptyFileList()
    {
        var diff = ""; // Empty diff -> 0 files
        var patch = GitPatch.FromApi(diff, "sha1234");
        var changeSet = new ChangeSet("source", patch);
        var fp = patch.Fingerprint[..7];

        Assert.Equal($"ChangeSet [sha1234:{fp}]: Produced patch", changeSet.GetSummary());
    }

    [Fact(DisplayName = "ChangeSet should handle partial common path.")]
    public void ShouldHandlePartialCommonPath()
    {
        // 6 files:
        // src/A/1.cs
        // src/B/2.cs
        // Common is "src/"
        var diff = "";
        for (int i = 0; i < 3; i++) diff += $"+++ b/src/A/{i}.cs\n";
        for (int i = 3; i < 6; i++) diff += $"+++ b/src/B/{i}.cs\n";

        var patch = GitPatch.FromApi(diff, "sha1234");
        var changeSet = new ChangeSet("source", patch);
        var fp = patch.Fingerprint[..7];

        Assert.Equal($"ChangeSet [sha1234:{fp}]: 6 src/* modified", changeSet.GetSummary());
    }
}
