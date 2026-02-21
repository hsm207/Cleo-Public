using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Tests.Persistence.Internal;

internal sealed class TestSessionPathResolver : ISessionPathResolver
{
    private readonly string _root;

    public TestSessionPathResolver(string root)
    {
        _root = root;
    }

    public string GetSessionsRoot() => _root;
}
