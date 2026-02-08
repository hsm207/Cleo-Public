using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Tests.Persistence;

public class DefaultRegistryPathProviderTests
{
    [Fact(DisplayName = "GetRegistryPath should return a path within the user's LocalApplicationData.")]
    public void GetRegistryPath_ShouldBeInAppData()
    {
        // Arrange
        var provider = new DefaultRegistryPathProvider();
        var expectedBase = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        // Act
        var result = provider.GetRegistryPath();

        // Assert
        Assert.Contains(expectedBase, result);
        Assert.Contains("Cleo", result);
        Assert.Contains("sessions.json", result);
    }
}
