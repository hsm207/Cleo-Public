using Cleo.Core.Domain.Services;
using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.Services;

public class AuthoritativePrResolverTests
{
    private readonly AuthoritativePrResolver _resolver = new();

    private readonly PullRequest _localPr = new(new Uri("https://github.com/pr/1"), "Local PR", "Desc", "head", "base");
    private readonly PullRequest _remotePr = new(new Uri("https://github.com/pr/2"), "Remote PR", "Desc", "head", "base");

    [Fact(DisplayName = "Resolve should return remote PR if both are present.")]
    public void ResolveShouldPrioritizeRemote()
    {
        var result = _resolver.Resolve(_localPr, _remotePr);
        Assert.Equal(_remotePr, result);
    }

    [Fact(DisplayName = "Resolve should return remote PR if only remote is present.")]
    public void ResolveShouldReturnRemote()
    {
        var result = _resolver.Resolve(null, _remotePr);
        Assert.Equal(_remotePr, result);
    }

    [Fact(DisplayName = "Resolve should return local PR if only local is present.")]
    public void ResolveShouldReturnLocal()
    {
        var result = _resolver.Resolve(_localPr, null);
        Assert.Equal(_localPr, result);
    }

    [Fact(DisplayName = "Resolve should return null if neither is present.")]
    public void ResolveShouldReturnNull()
    {
        var result = _resolver.Resolve(null, null);
        Assert.Null(result);
    }
}
