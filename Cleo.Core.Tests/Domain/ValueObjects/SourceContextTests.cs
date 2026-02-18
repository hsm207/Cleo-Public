using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class SourceContextTests
{
    [Fact(DisplayName = "A SourceContext should correctly capture both Repository and StartingBranch.")]
    public void ConstructorShouldSetValuesWhenValid()
    {
        var repo = "sources/github/hsm207/Cleo";
        var branch = "main";
        var context = new SourceContext(repo, branch);
        
        Assert.Equal(repo, context.Repository);
        Assert.Equal(branch, context.StartingBranch);
    }

    [Theory(DisplayName = "A SourceContext should throw an error if either repository or branch is missing.")]
    [InlineData("", "main")]
    [InlineData("sources/repo", "")]
    [InlineData(null, "main")]
    [InlineData("sources/repo", null)]
    public void ConstructorShouldThrowWhenInvalid(string? repo, string? branch)
    {
        Assert.Throws<ArgumentException>(() => new SourceContext(repo!, branch!));
    }

    [Theory(DisplayName = "SourceContext should enforce repository prefix.")]
    [InlineData("hsm207/Cleo")]
    [InlineData("source/hsm207/Cleo")] // missing 's'
    [InlineData("github/hsm207/Cleo")]
    public void ConstructorShouldThrowWhenPrefixInvalid(string repo)
    {
        var ex = Assert.Throws<ArgumentException>(() => new SourceContext(repo, "main"));
        Assert.Contains("must start with 'sources/'", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
