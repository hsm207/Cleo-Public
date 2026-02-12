using System.Net;
using Cleo.Core.Domain.Exceptions;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

namespace Cleo.Infrastructure.Tests.Clients.Jules;

public class RestSessionControllerTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock = new();
    private readonly RestSessionController _sut;

    public RestSessionControllerTests()
    {
        var httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://jules.ai")
        };
        _sut = new RestSessionController(httpClient);
    }

    [Fact(DisplayName = "ApprovePlan should throw DomainException on 400 Bad Request.")]
    public async Task ApprovePlan_ShouldThrow_On_400()
    {
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest });

        var act = async () => await _sut.ApprovePlanAsync(new SessionId("s"), CancellationToken.None);

        await act.Should().ThrowAsync<RemoteCollaboratorUnavailableException>();
    }

    [Fact(DisplayName = "ApprovePlan should succeed on 200 OK.")]
    public async Task ApprovePlan_ShouldSucceed()
    {
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        await _sut.ApprovePlanAsync(new SessionId("s"), CancellationToken.None);

        _handlerMock.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }
}
