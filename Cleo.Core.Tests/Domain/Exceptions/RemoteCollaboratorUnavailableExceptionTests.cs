using Cleo.Core.Domain.Exceptions;
using Xunit;

namespace Cleo.Core.Tests.Domain.Exceptions;

internal sealed class RemoteCollaboratorUnavailableExceptionTests
{
    [Fact(DisplayName = "The exception should support standard initialization patterns to satisfy platform requirements.")]
    public void ShouldSupportStandardPatterns()
    {
        // 1. Default (Platform mandated)
        var e1 = new RemoteCollaboratorUnavailableException();
        Assert.Contains("unreachable", e1.Message, StringComparison.OrdinalIgnoreCase);

        // 2. Custom message (Domain preference)
        var e2 = new RemoteCollaboratorUnavailableException("custom error");
        Assert.Equal("custom error", e2.Message);

        // 3. Wrapped (LSP Requirement)
        var inner = new InvalidOperationException("inner");
        var e3 = new RemoteCollaboratorUnavailableException("wrapped", inner);
        Assert.Equal("wrapped", e3.Message);
        Assert.Equal(inner, e3.InnerException);
    }
}
