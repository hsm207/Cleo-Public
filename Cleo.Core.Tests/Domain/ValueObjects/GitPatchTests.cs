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

    [Fact(DisplayName = "GitPatch should throw if BaseCommitId is empty.")]
    public void ShouldThrowIfBaseCommitIdEmpty()
    {
        Assert.Throws<ArgumentException>(() => new GitPatch("diff", ""));
        Assert.Throws<ArgumentException>(() => new GitPatch("diff", " "));
        Assert.Throws<ArgumentNullException>(() => new GitPatch("diff", null!));
    }
}
