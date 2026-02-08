using Cleo.Core.Domain.Ports;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Common;
using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net.Http; // Added

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

    [Fact(DisplayName = "AddCleoInfrastructure should register DpapiEncryptionStrategy when on Windows.")]
    public void DI_ShouldRegisterDpapiOnWindows()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockPlatform = new Mock<IPlatformProvider>();
        mockPlatform.Setup(p => p.IsWindows()).Returns(true);
        
        // Act
        services.AddCleoInfrastructure(new Uri("https://jules.googleapis.com/"), mockPlatform.Object);
        using var provider = services.BuildServiceProvider();

        // Assert
        var strategy = provider.GetRequiredService<IEncryptionStrategy>();
        Assert.IsType<DpapiEncryptionStrategy>(strategy);
    }

    [Fact(DisplayName = "DefaultPlatformProvider should accurately report the current OS.")]
    public void DefaultPlatformProvider_ShouldWork()
    {
        var provider = new DefaultPlatformProvider();
        // We can't assert true/false because it depends on the environment, 
        // but we can ensure it doesn't throw and matches RuntimeInformation.
        var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        Assert.Equal(isWindows, provider.IsWindows());
    }

    [Fact(DisplayName = "AddCleoInfrastructure should throw ArgumentNullException if services or URL are null.")]
    public void AddCleoInfrastructure_ShouldEnforceNullChecks()
    {
        var services = new ServiceCollection();
        var url = new Uri("https://jules.com");

        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddCleoInfrastructure(url));
        Assert.Throws<ArgumentNullException>(() => services.AddCleoInfrastructure(null!));
    }

    [Fact(DisplayName = "AddCleoInfrastructure should correctly configure HttpClient base address and headers for all clients.")]
    public void DI_ShouldConfigureHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var julesBaseUrl = new Uri("https://jules.googleapis.com/");
        services.AddCleoInfrastructure(julesBaseUrl);
        services.AddLogging();
        using var provider = services.BuildServiceProvider();

        // Act & Assert
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        
        var clientNames = new[] 
        { 
            typeof(IJulesSessionClient).Name, 
            typeof(IJulesSourceClient).Name, 
            typeof(IJulesActivityClient).Name 
        };

        foreach (var name in clientNames)
        {
            var client = factory.CreateClient(name);
            Assert.Equal(julesBaseUrl, client.BaseAddress);
            Assert.Contains(client.DefaultRequestHeaders.Accept, h => h.MediaType == "application/json");
        }
    }
}
