using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class SourceContextTests
{
    [Fact(DisplayName = "A SourceContext should correctly capture both Repository and StartingBranch.")]
    public void ConstructorShouldSetValuesWhenValid()
    {
        var repo = "hsm207/Cleo";
        var branch = "main";
        var context = new SourceContext(repo, branch);
        
        Assert.Equal(repo, context.Repository);
        Assert.Equal(branch, context.StartingBranch);
    }

    [Theory(DisplayName = "A SourceContext should throw an error if either repository or branch is missing.")]
    [InlineData("", "main")]
    [InlineData("repo", "")]
    [InlineData(null, "main")]
    [InlineData("repo", null)]
    public void ConstructorShouldThrowWhenInvalid(string? repo, string? branch)
    {
        Assert.Throws<ArgumentException>(() => new SourceContext(repo!, branch!));
    }
}
