using System.Net;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Moq;
using Moq.Protected;
using Xunit;

namespace Cleo.Infrastructure.Tests.Clients.Jules;

public class RestSessionMessengerTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock = new();
    private readonly RestSessionMessenger _messenger;
    private readonly SessionId _id = new("session-123");

    public RestSessionMessengerTests()
    {
        var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("https://jules.googleapis.com/") };
        _messenger = new RestSessionMessenger(httpClient);
    }

    [Fact(DisplayName = "SendMessage: Uses 'JulesSendMessageRequestDto' DTO to satisfy OCP.")]
    public async Task SendMessage_UsesCorrectDtoSchema()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        // Act
        await _messenger.SendMessageAsync(_id, "Make it pop", CancellationToken.None);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Method == HttpMethod.Post && 
                req.RequestUri!.ToString().EndsWith(":sendMessage") &&
                // Verify strict JSON contract matching the new DTO
                req.Content!.ReadAsStringAsync().Result.Contains("\"prompt\":\"Make it pop\"")
            ),
            ItExpr.IsAny<CancellationToken>());
    }
}
