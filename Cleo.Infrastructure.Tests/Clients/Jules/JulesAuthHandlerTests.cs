using System.Net;
using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Moq;
using Moq.Protected;

namespace Cleo.Infrastructure.Tests.Clients.Jules;

public class JulesAuthHandlerTests
{
    private readonly Mock<IVault> _vaultMock = new();
    private readonly Mock<HttpMessageHandler> _innerHandlerMock = new();
    private readonly JulesAuthHandler _handler;

    public JulesAuthHandlerTests()
    {
        _handler = new JulesAuthHandler(_vaultMock.Object)
        {
            InnerHandler = _innerHandlerMock.Object
        };
    }

    [Fact(DisplayName = "SendAsync should add the API key header when identity is found in the vault.")]
    public async Task SendAsync_ShouldAddApiKeyHeader_WhenIdentityFound()
    {
        // Arrange
        var apiKey = new ApiKey("test-api-key");
        var identity = new Identity(apiKey);
        identity.UpdateStatus(IdentityStatus.Valid);
        _vaultMock.Setup(v => v.RetrieveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(identity);

        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var client = new HttpClient(_handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(request.Headers.Contains("x-goog-api-key"));
        Assert.Equal("test-api-key", request.Headers.GetValues("x-goog-api-key").First());
    }

    [Fact(DisplayName = "SendAsync should not add the API key header when no identity is found.")]
    public async Task SendAsync_ShouldNotAddHeader_WhenNoIdentityFound()
    {
        // Arrange
        _vaultMock.Setup(v => v.RetrieveAsync(It.IsAny<CancellationToken>())).ReturnsAsync((Identity?)null);

        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var client = new HttpClient(_handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(request.Headers.Contains("x-goog-api-key"));
    }

    [Fact(DisplayName = "Constructor should throw when vault is null.")]
    public void Constructor_ShouldThrow_WhenVaultIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new JulesAuthHandler(null!));
    }
}
