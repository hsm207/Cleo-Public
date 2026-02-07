using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class SolutionPatchTests
{
    [Fact(DisplayName = "A SolutionPatch should correctly capture diff, base commit, and optional message.")]
    public void ConstructorShouldSetValuesWhenValid()
    {
        var diff = "--- a/file.txt\n+++ b/file.txt";
        var commit = "abc12345";
        var msg = "Fix stuff";
        var patch = new SolutionPatch(diff, commit, msg);

        Assert.Equal(diff, patch.UniDiff);
        Assert.Equal(commit, patch.BaseCommitId);
        Assert.Equal(msg, patch.SuggestedCommitMessage);
    }

    [Theory(DisplayName = "A SolutionPatch should throw an error if critical fields are missing.")]
    [InlineData("", "abc")]
    [InlineData("diff", "")]
    [InlineData(null, "abc")]
    [InlineData("diff", null)]
    public void ConstructorShouldThrowWhenInvalid(string? diff, string? commit)
    {
        Assert.Throws<ArgumentException>(() => new SolutionPatch(diff!, commit!));
    }
}
