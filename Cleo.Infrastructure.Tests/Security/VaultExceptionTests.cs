using Cleo.Infrastructure.Security;
using FluentAssertions;
using Xunit;

namespace Cleo.Infrastructure.Tests.Security;

public sealed class VaultExceptionTests
{
    [Fact(DisplayName = "VaultSecurityException should support all standard constructor patterns.")]
    public void ShouldSupportAllConstructors()
    {
        // 1. Default
        var ex1 = new VaultSecurityException();
        ex1.Message.Should().NotBeNull();

        // 2. Message
        var ex2 = new VaultSecurityException("test");
        ex2.Message.Should().Be("test");

        // 3. Inner Exception
        var inner = new Exception("inner");
        var ex3 = new VaultSecurityException("test", inner);
        ex3.Message.Should().Be("test");
        ex3.InnerException.Should().Be(inner);
    }
}
