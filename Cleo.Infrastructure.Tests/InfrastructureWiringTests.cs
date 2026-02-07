using Cleo.Core.Domain.Ports;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Cleo.Infrastructure.Tests;

public class InfrastructureWiringTests
{
    [Fact(DisplayName = "The DI container should resolve all segregated infrastructure ports.")]
    public void DI_ShouldResolveAllPorts()
    {
        // Arrange
        var services = new ServiceCollection();
        var julesBaseUrl = new Uri("https://jules.googleapis.com/");
        
        // Act
        services.AddCleoInfrastructure(julesBaseUrl);
        services.AddLogging(); // Required by some handlers
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetRequiredService<IVault>());
        Assert.NotNull(provider.GetRequiredService<ISessionReader>());
        Assert.NotNull(provider.GetRequiredService<ISessionWriter>());
        Assert.NotNull(provider.GetRequiredService<IDispatcher>());
        Assert.NotNull(provider.GetRequiredService<IJulesSessionClient>());
        Assert.NotNull(provider.GetRequiredService<IJulesSourceClient>());
        Assert.NotNull(provider.GetRequiredService<IJulesActivityClient>());
    }

    [Fact(DisplayName = "Specialized clients should resolve to their respective implementations.")]
    public void DI_ShouldResolveSpecializedClients()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCleoInfrastructure(new Uri("https://jules.googleapis.com/"));
        services.AddLogging();
        using var provider = services.BuildServiceProvider();

        // Act
        var sessionClient = provider.GetRequiredService<IJulesSessionClient>();
        var sourceClient = provider.GetRequiredService<IJulesSourceClient>();
        var activityClient = provider.GetRequiredService<IJulesActivityClient>();

        // Assert
        Assert.IsType<RestJulesSessionClient>(sessionClient);
        Assert.IsType<RestJulesSourceClient>(sourceClient);
        Assert.IsType<RestJulesActivityClient>(activityClient);
    }
}
