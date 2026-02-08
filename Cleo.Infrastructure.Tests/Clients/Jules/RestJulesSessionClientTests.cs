using System.Net;
using System.Net.Http.Json;
using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Exceptions;
using Cleo.Core.Domain.Ports;
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
    private readonly ISessionStatusMapper _statusMapper = new DefaultSessionStatusMapper();
    private readonly RestJulesSessionClient _client;
    private readonly SessionId _testId = new("sessions/123");

    public RestJulesSessionClientTests()
    {
        var httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://jules.googleapis.com/")
        };
        _client = new RestJulesSessionClient(httpClient, _statusMapper);
    }

    [Fact(DisplayName = "CreateSessionAsync should post a session request and return a mapped session.")]
    public async Task CreateSessionAsync_ShouldReturnSession()
    {
        // Arrange
        var task = new TaskDescription("task");
        var source = new SourceContext("repo", "branch");
        var options = new SessionCreationOptions();
        var dto = new JulesSessionDto("name", "id", "QUEUED", "prompt", new SourceContextDto("repo", new GithubRepoContextDto("branch")), null, true, "AUTO_CREATE_PR", null, null);

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(dto)
            });

        // Act
        var result = await _client.CreateSessionAsync(task, source, options, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("name", result.Id.Value);
        Assert.Equal(SessionStatus.StartingUp, result.Pulse.Status);
    }

    [Fact(DisplayName = "GetSessionPulseAsync should return a mapped session pulse.")]
    public async Task GetSessionPulseAsync_ShouldReturnPulse()
    {
        // Arrange
        var dto = new JulesSessionDto("name", "id", "PLANNING", "prompt", new SourceContextDto("repo", null), null, true, "NONE", null, null);

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(dto)
            });

        // Act
        var result = await _client.GetSessionPulseAsync(_testId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(SessionStatus.Planning, result.Status);
    }

    [Fact(DisplayName = "SendMessageAsync should post a message request with the correct 'prompt' field.")]
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
        await _client.SendMessageAsync(_testId, "hello world", TestContext.Current.CancellationToken);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Method == HttpMethod.Post && 
                req.Content != null && 
                req.Content.ReadAsStringAsync().Result.Contains("\"prompt\":\"hello world\"")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact(DisplayName = "The client should include status code and response body in the exception on failure.")]
    public async Task ShouldIncludeResponseDetailsOnFailure()
    {
        // Arrange
        var errorBody = "{\"error\": \"details\"}";
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(errorBody)
            });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<RemoteCollaboratorUnavailableException>(() => 
            _client.SendMessageAsync(_testId, "fail", TestContext.Current.CancellationToken));
        
        Assert.Contains("400", ex.Message);
        Assert.Contains(errorBody, ex.Message);
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

    [Fact(DisplayName = "CreateSessionAsync should throw RemoteCollaboratorUnavailableException on connectivity failure.")]
    public async Task CreateSessionAsync_ShouldThrowOnConnectivityFailure()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException());

        // Act & Assert
        await Assert.ThrowsAsync<RemoteCollaboratorUnavailableException>(() => _client.CreateSessionAsync(new TaskDescription("t"), new SourceContext("r", "b"), new SessionCreationOptions(), TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "GetSessionPulseAsync should throw RemoteCollaboratorUnavailableException on connectivity failure.")]
    public async Task GetSessionPulseAsync_ShouldThrowOnConnectivityFailure()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException());

        // Act & Assert
        await Assert.ThrowsAsync<RemoteCollaboratorUnavailableException>(() => _client.GetSessionPulseAsync(_testId, TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "GetSessionPulseAsync should throw InvalidOperationException on null response.")]
    public async Task GetSessionPulseAsync_ShouldThrowOnNullResponse()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
            });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _client.GetSessionPulseAsync(_testId, TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "ApprovePlanAsync should throw RemoteCollaboratorUnavailableException on connectivity failure.")]
    public async Task ApprovePlanAsync_ShouldThrowOnConnectivityFailure()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException());

        // Act & Assert
        await Assert.ThrowsAsync<RemoteCollaboratorUnavailableException>(() => _client.ApprovePlanAsync(_testId, TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "The client should throw if the server returns an error.")]
    public async Task ShouldThrowOnServerError()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act & Assert
        await Assert.ThrowsAsync<RemoteCollaboratorUnavailableException>(() => _client.SendMessageAsync(_testId, "fail", TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "The client should correctly implement IPulseMonitor and ISessionMessenger ports.")]
    public async Task ShouldImplementPorts()
    {
        // Arrange
        var messenger = (ISessionMessenger)_client;
        var monitor = (IPulseMonitor)_client;
        var dto = new JulesSessionDto("name", "id", "PLANNING", "prompt", new SourceContextDto("repo", null), null, true, "NONE", null, null);

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(dto)
            });

        // Act
        await messenger.SendMessageAsync(_testId, "hi", TestContext.Current.CancellationToken);
        var pulse = await monitor.GetSessionPulseAsync(_testId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(SessionStatus.Planning, pulse.Status);
    }
}
