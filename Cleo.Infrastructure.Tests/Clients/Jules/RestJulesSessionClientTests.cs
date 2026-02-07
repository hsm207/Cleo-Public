using System.Net;
using System.Net.Http.Json;
using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using Moq;
using Moq.Protected;

namespace Cleo.Infrastructure.Tests.Clients.Jules;

public class RestJulesSessionClientTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock = new();
    private readonly Mock<ISessionStatusMapper> _statusMapperMock = new();
    private readonly RestJulesSessionClient _client;
    private readonly SessionId _testId = new("sessions/123");

    public RestJulesSessionClientTests()
    {
        var httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://jules.googleapis.com/")
        };
        _client = new RestJulesSessionClient(httpClient, _statusMapperMock.Object);
    }

    [Fact(DisplayName = "CreateSessionAsync should post a session request and return a mapped session.")]
    public async Task CreateSessionAsync_ShouldReturnSession()
    {
        // Arrange
        var task = new TaskDescription("task");
        var source = new SourceContext("repo", "branch");
        var dto = new JulesSessionDto("name", "id", "QUEUED", "prompt", new SourceContextDto("repo", new GithubRepoContextDto("branch")));

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(dto)
            });

        _statusMapperMock.Setup(m => m.Map("QUEUED")).Returns(SessionStatus.StartingUp);

        // Act
        var result = await _client.CreateSessionAsync(task, source, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("name", result.Id.Value);
        Assert.Equal(SessionStatus.StartingUp, result.Pulse.Status);
    }

    [Fact(DisplayName = "GetSessionPulseAsync should return a mapped session pulse.")]
    public async Task GetSessionPulseAsync_ShouldReturnPulse()
    {
        // Arrange
        var dto = new JulesSessionDto("name", "id", "PLANNING", "prompt", new SourceContextDto("repo", null));

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(dto)
            });

        _statusMapperMock.Setup(m => m.Map("PLANNING")).Returns(SessionStatus.Planning);

        // Act
        var result = await _client.GetSessionPulseAsync(_testId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(SessionStatus.Planning, result.Status);
    }

    [Fact(DisplayName = "SendMessageAsync should post a message request.")]
    public async Task SendMessageAsync_ShouldPostMessage()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        await _client.SendMessageAsync(_testId, "hello", TestContext.Current.CancellationToken);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact(DisplayName = "ApprovePlanAsync should post an empty request to the approvePlan endpoint.")]
    public async Task ApprovePlanAsync_ShouldPostApproval()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        await _client.ApprovePlanAsync(_testId, TestContext.Current.CancellationToken);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri!.ToString().EndsWith(":approvePlan")),
            ItExpr.IsAny<CancellationToken>());
    }
}
