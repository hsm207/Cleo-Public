using System.Net;
using Cleo.Infrastructure.Clients.Jules;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Cleo.Infrastructure.Tests.Clients.Jules;

public class JulesLoggingHandlerTests
{
    // REAL VIBE: Official FakeLogger from Microsoft
    private readonly FakeLogger<JulesLoggingHandler> _logger = new();
    private readonly Mock<HttpMessageHandler> _innerHandlerMock = new();
    private readonly JulesLoggingHandler _handler;

    public JulesLoggingHandlerTests()
    {
        _handler = new JulesLoggingHandler(_logger)
        {
            InnerHandler = _innerHandlerMock.Object
        };
    }

    [Fact(DisplayName = "SendAsync should log information on successful response.")]
    public async Task SendAsync_ShouldLogInfo_OnSuccess()
    {
        // Arrange
        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var client = new HttpClient(_handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert: Verify intent using the FakeLogger collector
        var logs = _logger.Collector.GetSnapshot();
        
        Assert.Contains(logs, l => l.Level == LogLevel.Information && l.Message.Contains("Sending GET"));
        Assert.Contains(logs, l => l.Level == LogLevel.Information && l.Message.Contains("Received 200"));
    }

    [Fact(DisplayName = "SendAsync should log warning on unsuccessful status code.")]
    public async Task SendAsync_ShouldLogWarning_OnFailureCode()
    {
        // Arrange
        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        var client = new HttpClient(_handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        var logs = _logger.Collector.GetSnapshot();
        Assert.Contains(logs, l => l.Level == LogLevel.Warning && l.Message.Contains("Received 404"));
    }

    [Fact(DisplayName = "SendAsync should log error on exception.")]
    public async Task SendAsync_ShouldLogError_OnException()
    {
        // Arrange
        var exception = new HttpRequestException("Boom!");
        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(exception);

        var client = new HttpClient(_handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => client.SendAsync(request, TestContext.Current.CancellationToken));

        var logs = _logger.Collector.GetSnapshot();
        Assert.Contains(logs, l => l.Level == LogLevel.Error && l.Message.Contains("failed"));
        Assert.Equal(exception, logs.First(l => l.Level == LogLevel.Error).Exception);
    }

    [Fact(DisplayName = "Constructor should throw when logger is null.")]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new JulesLoggingHandler(null!));
    }
}
