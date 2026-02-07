using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Moq;
using Xunit;

namespace Cleo.Core.Tests.Domain.Ports;

public class VaultTests
{
    [Fact(DisplayName = "The IVault port should define a clear contract for storing, retrieving, and clearing identity.")]
    public async Task VaultPortShouldDefineStandardOperations()
    {
        // We aren't testing an implementation, we're verifying the PORT's usage design!
        var mockVault = new Mock<IVault>();
        var testIdentity = new Identity(new ApiKey("test-key"));

        // Verify Store
        await mockVault.Object.StoreAsync(testIdentity, TestContext.Current.CancellationToken);
        mockVault.Verify(v => v.StoreAsync(testIdentity, It.IsAny<CancellationToken>()), Times.Once);

        // Verify Retrieve
        mockVault.Setup(v => v.RetrieveAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(testIdentity);
        var retrieved = await mockVault.Object.RetrieveAsync(TestContext.Current.CancellationToken);
        Assert.Equal(testIdentity, retrieved);

        // Verify Clear
        await mockVault.Object.ClearAsync(TestContext.Current.CancellationToken);
        mockVault.Verify(v => v.ClearAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
