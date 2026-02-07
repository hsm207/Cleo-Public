using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class SessionSourceTests
{
    [Fact(DisplayName = "SessionSource should store name, owner, and repo.")]
    public void SessionSourceProperties()
    {
        var source = new SessionSource("name", "owner", "repo");
        Assert.Equal("name", source.Name);
        Assert.Equal("owner", source.Owner);
        Assert.Equal("repo", source.Repo);
    }
}
