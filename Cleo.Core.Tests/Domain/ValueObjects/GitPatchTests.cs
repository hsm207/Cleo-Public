using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public sealed class GitPatchTests
{
    [Fact(DisplayName = "GitPatch should allow empty UniDiff to support agent startup heartbeats.")]
    public void ShouldAllowEmptyDiff()
    {
        var patch1 = GitPatch.FromApi("", "sha");
        var patch2 = GitPatch.FromApi(" ", "sha");

        Assert.Equal("", patch1.UniDiff);
        Assert.Equal(" ", patch2.UniDiff);
        
        // FromApi allows null and treats it as empty string
        var patch3 = GitPatch.FromApi(null!, "sha");
        Assert.Equal("", patch3.UniDiff);
    }

    [Fact(DisplayName = "GitPatch should throw if baseCommitId is missing to maintain domain integrity.")]
    public void ShouldThrowIfBaseCommitIdInvalid()
    {
        Assert.Throws<ArgumentNullException>(() => GitPatch.FromApi("diff", null!));
        Assert.Throws<ArgumentException>(() => GitPatch.FromApi("diff", ""));
        Assert.Throws<ArgumentException>(() => GitPatch.FromApi("diff", "  "));
    }

    [Fact(DisplayName = "GitPatch should extract unique modified filenames from UniDiff to enable impact analysis.")]
    public void ShouldExtractFilenames()
    {
        var diff = @"diff --git a/file1.cs b/file1.cs
--- a/file1.cs
+++ b/file1.cs
@@ -1,1 +1,2 @@
+new line
diff --git a/dir/file2.md b/dir/file2.md
--- a/dir/file2.md
+++ b/dir/file2.md
";
        var patch = GitPatch.FromApi(diff, "sha");

        var files = patch.GetModifiedFiles();
        Assert.Equal(2, files.Count);
        Assert.Contains("file1.cs", files);
        Assert.Contains("dir/file2.md", files);
    }

    [Fact(DisplayName = "GitPatch should handle /dev/null in filenames during extraction.")]
    public void ShouldHandleDevNull()
    {
        var diff = @"--- /dev/null
+++ b/newfile.txt
";
        var patch = GitPatch.FromApi(diff, "sha");

        var files = patch.GetModifiedFiles();
        Assert.Single(files);
        Assert.Equal("newfile.txt", files[0]);
    }

    [Fact(DisplayName = "GitPatch should calculate a deterministic fingerprint when created from the API.")]
    public void ShouldCalculateFingerprintWhenCreatedFromApi()
    {
        var patch = GitPatch.FromApi("diff content", "sha123");
        Assert.NotNull(patch.Fingerprint);
        Assert.NotEmpty(patch.Fingerprint);
        Assert.Equal(32, patch.Fingerprint.Length); // XxHash128 is 128-bit hex (32 chars)
    }

    [Fact(DisplayName = "GitPatch should use the provided fingerprint when restored to preserve ledger history.")]
    public void ShouldUseProvidedFingerprintWhenRestored()
    {
        var fingerprint = "deadbeef";
        var patch = GitPatch.Restore("diff content", "sha123", fingerprint);
        Assert.Equal(fingerprint, patch.Fingerprint);
    }

    [Fact(DisplayName = "GitPatch should throw if fingerprint is missing during restoration to prevent history corruption.")]
    public void ShouldThrowIfFingerprintMissingDuringRestoration()
    {
        Assert.Throws<ArgumentNullException>(() => GitPatch.Restore("diff", "sha", null!));
        Assert.Throws<ArgumentException>(() => GitPatch.Restore("diff", "sha", ""));
        Assert.Throws<ArgumentException>(() => GitPatch.Restore("diff", "sha", "   "));
    }

    [Fact(DisplayName = "GitPatch should produce a unique content identity for different UniDiff contents.")]
    public void ShouldProduceUniqueContentIdentity()
    {
        var patch1 = GitPatch.FromApi("diff A", "sha");
        var patch2 = GitPatch.FromApi("diff B", "sha");

        Assert.NotEqual(patch1.Fingerprint, patch2.Fingerprint);
    }

    [Fact(DisplayName = "GitPatch should produce a stable fingerprint for the same UniDiff content.")]
    public void ShouldProduceStableFingerprint()
    {
        var patch1 = GitPatch.FromApi("diff A", "sha");
        var patch2 = GitPatch.FromApi("diff A", "sha");

        Assert.Equal(patch1.Fingerprint, patch2.Fingerprint);
    }
}
