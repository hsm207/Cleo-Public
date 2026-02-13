using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class GitPatchRegexTests
{
    [Theory(DisplayName = "GitPatch regex should extract valid filenames.")]
    [InlineData("+++ b/file.txt", "file.txt")]
    [InlineData("+++ b/path/to/file.cs", "path/to/file.cs")]
    [InlineData("+++ b/file with spaces.txt", "file with spaces.txt")]
    [InlineData("+++ b/file_with_underscore.js", "file_with_underscore.js")]
    [InlineData("+++ b/file-with-dash.py", "file-with-dash.py")]
    [InlineData("+++ b/MixedCaseFile.TS", "MixedCaseFile.TS")]
    public void ShouldExtractFilename(string line, string expectedFilename)
    {
        var diff = line;
        var patch = new GitPatch(diff, "sha");
        var files = patch.GetModifiedFiles();

        Assert.Single(files);
        Assert.Equal(expectedFilename, files[0]);
    }

    [Theory(DisplayName = "GitPatch regex should ignore invalid lines.")]
    [InlineData("+++ /dev/null")] // Deletion (doesn't start with b/)
    [InlineData("--- a/file.txt")]
    [InlineData("+++ a/file.txt")] // Wrong prefix
    [InlineData("index 1234567..8901234")]
    [InlineData("@@ -1,2 +3,4 @@")]
    [InlineData("")]
    public void ShouldIgnoreInvalidLines(string line)
    {
        var diff = line;
        var patch = new GitPatch(diff, "sha");
        var files = patch.GetModifiedFiles();

        Assert.Empty(files);
    }

    [Fact(DisplayName = "GitPatch regex should handle multiple files in one diff.")]
    public void ShouldHandleMultipleFiles()
    {
        var diff = @"
diff --git a/file1.cs b/file1.cs
index ...
--- a/file1.cs
+++ b/file1.cs
@@ ...
diff --git a/file2.md b/file2.md
index ...
--- a/file2.md
+++ b/file2.md
@@ ...
";
        var patch = new GitPatch(diff, "sha");
        var files = patch.GetModifiedFiles();

        Assert.Equal(2, files.Count);
        Assert.Contains("file1.cs", files);
        Assert.Contains("file2.md", files);
    }

    [Fact(DisplayName = "GitPatch regex should deduplicate filenames.")]
    public void ShouldDeduplicateFilenames()
    {
        var diff = @"
+++ b/file1.cs
...
+++ b/file1.cs
";
        var patch = new GitPatch(diff, "sha");
        var files = patch.GetModifiedFiles();

        Assert.Single(files);
        Assert.Equal("file1.cs", files[0]);
    }
}
