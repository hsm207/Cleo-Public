using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

internal class PullRequestTests
{
    [Fact(DisplayName = "PullRequest should store valid values.")]
    public void ConstructorShouldStoreValues()
    {
        var url = new Uri("https://github.com/org/repo/pull/1");
        var title = "Fix bug";
        var description = "Fixed the bug.";
        var headRef = "feature-branch";
        var baseRef = "main";

        var pr = new PullRequest(url, title, description, headRef, baseRef);

        Assert.Equal(url, pr.Url);
        Assert.Equal(title, pr.Title);
        Assert.Equal(description, pr.Description);
        Assert.Equal(headRef, pr.HeadRef);
        Assert.Equal(baseRef, pr.BaseRef);
    }

    [Fact(DisplayName = "PullRequest should throw ArgumentNullException if URL is null.")]
    public void ConstructorShouldThrowOnNullUrl()
    {
        Assert.Throws<ArgumentNullException>(() => new PullRequest(null!, "Title", "Desc", "head", "base"));
    }

    [Fact(DisplayName = "PullRequest should throw ArgumentException if Title is null or whitespace.")]
    public void ConstructorShouldThrowOnInvalidTitle()
    {
        var url = new Uri("https://github.com");
        Assert.Throws<ArgumentException>(() => new PullRequest(url, null!, "Desc", "head", "base"));
        Assert.Throws<ArgumentException>(() => new PullRequest(url, " ", "Desc", "head", "base"));
    }

    [Fact(DisplayName = "PullRequest should throw ArgumentException if Description is missing.")]
    public void ConstructorShouldThrowOnMissingDescription()
    {
        var url = new Uri("https://github.com");
        Assert.Throws<ArgumentException>(() => new PullRequest(url, "Title", null!, "head", "base"));
        Assert.Throws<ArgumentException>(() => new PullRequest(url, "Title", "", "head", "base"));
        Assert.Throws<ArgumentException>(() => new PullRequest(url, "Title", "   ", "head", "base"));
    }

    [Fact(DisplayName = "PullRequest should throw ArgumentException if HeadRef is missing.")]
    public void ConstructorShouldThrowOnMissingHeadRef()
    {
        var url = new Uri("https://github.com");
        Assert.Throws<ArgumentException>(() => new PullRequest(url, "Title", "Desc", null!, "base"));
        Assert.Throws<ArgumentException>(() => new PullRequest(url, "Title", "Desc", "", "base"));
    }

    [Fact(DisplayName = "PullRequest should throw ArgumentException if BaseRef is missing.")]
    public void ConstructorShouldThrowOnMissingBaseRef()
    {
        var url = new Uri("https://github.com");
        Assert.Throws<ArgumentException>(() => new PullRequest(url, "Title", "Desc", "head", null!));
        Assert.Throws<ArgumentException>(() => new PullRequest(url, "Title", "Desc", "head", ""));
    }
}
