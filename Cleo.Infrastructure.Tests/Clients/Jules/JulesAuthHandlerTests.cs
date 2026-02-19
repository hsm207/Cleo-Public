using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Security;
using Moq;
using Moq.Protected;
using Xunit;

namespace Cleo.Infrastructure.Tests.Clients.Jules;

public class JulesAuthHandlerTests : IDisposable
{
    private readonly string _tempFile = Path.GetTempFileName();
    private readonly NativeVault _vault;
    private readonly Mock<HttpMessageHandler> _innerHandlerMock = new();

    public JulesAuthHandlerTests()
    {
        // REAL VIBES: Use real vault with real encryption
        _vault = new NativeVault(_tempFile, new AesGcmEncryptionStrategy());
    }

    [Fact(DisplayName = "JulesAuthHandler should add the x-goog-api-key header from the real vault.")]
    public async Task SendAsync_ShouldAddApiKeyHeader()
    {
        // Arrange: Store a real key in the vault
        var apiKey = "real-secret-key";
        var identity = new Identity(new ApiKey(apiKey));
        await _vault.StoreAsync(identity, TestContext.Current.CancellationToken);

        var handler = new JulesAuthHandler(_vault)
        {
            InnerHandler = _innerHandlerMock.Object
        };

        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        _innerHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Headers.Contains("x-goog-api-key") &&
                req.Headers.GetValues("x-goog-api-key").First() == apiKey),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact(DisplayName = "JulesAuthHandler should not add the header if the vault is empty.")]
    public async Task SendAsync_ShouldNotAddHeaderIfVaultEmpty()
    {
        // Arrange: Vault is empty (ensure file is gone)
        if (File.Exists(_tempFile)) File.Delete(_tempFile);

        var handler = new JulesAuthHandler(_vault)
        {
            InnerHandler = _innerHandlerMock.Object
        };

        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        _innerHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => !req.Headers.Contains("x-goog-api-key")),
            ItExpr.IsAny<CancellationToken>());
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile)) File.Delete(_tempFile);
    }
}
