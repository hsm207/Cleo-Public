using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class PullRequestTests
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
        Assert.Throws<ArgumentNullException>(() => new PullRequest(null!, "Title"));
    }

    [Fact(DisplayName = "PullRequest should throw ArgumentException if Title is null or whitespace.")]
    public void ConstructorShouldThrowOnInvalidTitle()
    {
        var url = new Uri("https://github.com");
        Assert.Throws<ArgumentException>(() => new PullRequest(url, null!));
        Assert.Throws<ArgumentException>(() => new PullRequest(url, " "));
    }

    [Fact(DisplayName = "PullRequest should allow null description.")]
    public void ConstructorShouldAllowNullDescription()
    {
        var url = new Uri("https://github.com");
        var pr = new PullRequest(url, "Title", null);
        Assert.Null(pr.Description);
    }
}
