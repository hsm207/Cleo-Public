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
    private readonly RestSessionController _controller;
    private readonly SessionId _id = new("session-123");

    public RestSessionControllerTests()
    {
        var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("https://jules.googleapis.com/") };
        _controller = new RestSessionController(httpClient);
    }

    [Fact(DisplayName = "ApprovePlan: Uses explicit DTO and separates Control from Factory.")]
    public async Task ApprovePlan_SendsCorrectSignal()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        // Act
        await _controller.ApprovePlanAsync(_id, CancellationToken.None);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Method == HttpMethod.Post && 
                req.RequestUri!.ToString().EndsWith(":approvePlan")
            ),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact(DisplayName = "ApprovePlan: Throws RemoteCollaboratorUnavailableException on failure.")]
    public async Task ApprovePlan_ThrowsOnFailure()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

        // Act
        var act = async () => await _controller.ApprovePlanAsync(_id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RemoteCollaboratorUnavailableException>();
    }
}
