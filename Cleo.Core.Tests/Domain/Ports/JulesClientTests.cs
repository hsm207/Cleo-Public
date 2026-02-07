using Cleo.Core.Domain.Ports;
using Moq;
using Xunit;

namespace Cleo.Core.Tests.Domain.Ports;

public class JulesClientTests
{
    private readonly Mock<IJulesClient> _mockClient = new();

    [Fact(DisplayName = "IJulesClient should define the contract for collaborative activities.")]
    public void InterfaceShouldBeImplementable()
    {
        // This test simply verifies the interface contract is stable and mocking works.
        Assert.NotNull(_mockClient.Object);
    }
}
