using System.Net;
using Cleo.Infrastructure.Clients.Jules;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace Cleo.Infrastructure.Tests.Clients.Jules;

public class JulesLoggingHandlerTests
{
    private readonly Mock<ILogger<JulesLoggingHandler>> _loggerMock = new();
    private readonly Mock<HttpMessageHandler> _innerHandlerMock = new();
    private readonly JulesLoggingHandler _handler;

    public JulesLoggingHandlerTests()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _handler = new JulesLoggingHandler(_loggerMock.Object)
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

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending GET")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Received 200")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
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
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Received 404")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
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

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact(DisplayName = "Constructor should throw when logger is null.")]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new JulesLoggingHandler(null!));
    }
}
