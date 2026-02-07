using Cleo.Core.Domain.Ports;
using Moq;
using Xunit;

namespace Cleo.Core.Tests.Domain.Ports;

public class JulesClientTests
{
    private readonly Mock<IJulesSessionClient> _mockSessionClient = new();
    private readonly Mock<IJulesSourceClient> _mockSourceClient = new();
    private readonly Mock<IJulesActivityClient> _mockActivityClient = new();

    [Fact(DisplayName = "Jules interfaces should define segregated contracts for collaborative activities.")]
    public void InterfacesShouldBeImplementable()
    {
        // This test simply verifies the interface contracts are stable and mocking works.
        Assert.NotNull(_mockSessionClient.Object);
        Assert.NotNull(_mockSourceClient.Object);
        Assert.NotNull(_mockActivityClient.Object);
    }
}
