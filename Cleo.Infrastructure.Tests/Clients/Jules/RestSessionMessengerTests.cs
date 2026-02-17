using System.Net;
using Cleo.Core.Domain.Exceptions;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

namespace Cleo.Infrastructure.Tests.Clients.Jules;

public class RestSessionMessengerTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock = new();
    private readonly RestSessionMessenger _sut;

    public RestSessionMessengerTests()
    {
        var httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://jules.ai")
        };
        _sut = new RestSessionMessenger(httpClient);
    }

    [Fact(DisplayName = "SendMessage should throw DomainException on 500 Server Error.")]
    public async Task SendMessage_ShouldThrow_On_500()
    {
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

        var act = async () => await _sut.SendMessageAsync(new SessionId("sessions/s"), "msg", CancellationToken.None);

        await act.Should().ThrowAsync<RemoteCollaboratorUnavailableException>();
    }

    [Fact(DisplayName = "SendMessage should succeed on 200 OK.")]
    public async Task SendMessage_ShouldSucceed()
    {
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        await _sut.SendMessageAsync(new SessionId("sessions/s"), "Hello", CancellationToken.None);

        _handlerMock.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }
}
