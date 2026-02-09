using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class GitPatchTests
{
    [Fact(DisplayName = "GitPatch should be created with valid arguments.")]
    public void ShouldCreateWithValidArgs()
    {
        var patch = new GitPatch("diff content", "sha123", "feat: changes");

        Assert.Equal("diff content", patch.UniDiff);
        Assert.Equal("sha123", patch.BaseCommitId);
        Assert.Equal("feat: changes", patch.SuggestedCommitMessage);
    }

    [Fact(DisplayName = "GitPatch should allow null suggested commit message.")]
    public void ShouldAllowNullMessage()
    {
        var patch = new GitPatch("diff", "sha");
        Assert.Null(patch.SuggestedCommitMessage);
    }

    [Fact(DisplayName = "GitPatch should throw if UniDiff is empty.")]
    public void ShouldThrowIfDiffEmpty()
    {
        Assert.Throws<ArgumentException>(() => new GitPatch("", "sha"));
        Assert.Throws<ArgumentException>(() => new GitPatch(" ", "sha"));
        Assert.Throws<ArgumentNullException>(() => new GitPatch(null!, "sha"));
    }

    [Fact(DisplayName = "GitPatch should extract unique modified filenames from UniDiff.")]
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
        var patch = new GitPatch(diff, "sha");

        var files = patch.GetModifiedFiles();
        Assert.Equal(2, files.Count);
        Assert.Contains("file1.cs", files);
        Assert.Contains("dir/file2.md", files);
    }

    [Fact(DisplayName = "GitPatch should handle /dev/null in filenames.")]
    public void ShouldHandleDevNull()
    {
        var diff = @"--- /dev/null
+++ b/newfile.txt
";
        var patch = new GitPatch(diff, "sha");

        var files = patch.GetModifiedFiles();
        Assert.Single(files);
        Assert.Equal("newfile.txt", files[0]);
    }
}
