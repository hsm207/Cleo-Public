using System.Net;
using System.Net.Http.Json;
using Cleo.Core.Domain.Exceptions;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

namespace Cleo.Infrastructure.Tests.Clients.Jules;

public class RestPulseMonitorTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock = new();
    private readonly ISessionStatusMapper _statusMapper = new DefaultSessionStatusMapper();
    private readonly RestPulseMonitor _monitor;
    private readonly SessionId _id = new("sessions/session-123");
    private readonly TaskDescription _task = new("Fix bugs");

    public RestPulseMonitorTests()
    {
        var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("https://jules.googleapis.com/") };
        _monitor = new RestPulseMonitor(httpClient, _statusMapper);
    }

    [Fact(DisplayName = "GetSessionPulseAsync: Returns status without coupling to Creation logic.")]
    public async Task GetSessionPulseAsync_IsIsolatedAndCorrect()
    {
        // Arrange
        // JulesSessionResponseDto(Name, Id, State, Prompt, SourceContext, Url, RequirePlanApproval, AutomationMode, CreateTime, UpdateTime, Title, Outputs)
        var dto = new JulesSessionResponseDto("sessions/session-123", "id", JulesSessionState.InProgress, "prompt", new JulesSourceContextDto("sources/repo", null), null, true, JulesAutomationMode.AutomationModeUnspecified, null, null);
        
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create(dto) });

        // Act
        var result = await _monitor.GetSessionPulseAsync(_id, CancellationToken.None);

        // Assert
        result.Status.Should().Be(SessionStatus.InProgress);
        
        // Verify we used GET on the correct resource URI
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Method == HttpMethod.Get && 
                req.RequestUri!.ToString().Contains("session-123")
            ),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact(DisplayName = "GetSessionPulseAsync: Throws RemoteCollaboratorUnavailableException on network failure.")]
    public async Task GetSessionPulseAsync_ThrowsOnNetworkError()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network down"));

        // Act
        var act = async () => await _monitor.GetSessionPulseAsync(_id, CancellationToken.None);

        // Assert
        // Fixed: FluentAssertions syntax for checking inner exception
        (await act.Should().ThrowAsync<RemoteCollaboratorUnavailableException>())
            .WithInnerException<HttpRequestException>();
    }

    [Fact(DisplayName = "GetRemoteSessionAsync: Returns mapped session correctly.")]
    public async Task GetRemoteSessionAsync_ReturnsMappedSession()
    {
        // Arrange
        // JulesSourceContextDto(Source, GithubRepoContext, EnvironmentVariablesEnabled)
        var sourceDto = new JulesSourceContextDto("source", new JulesGithubRepoContextDto("sources/main"));
        var dto = new JulesSessionResponseDto("sessions/session-123", "rem-1", JulesSessionState.Completed, "Original Prompt", sourceDto, null, true, JulesAutomationMode.AutoCreatePr, null, null);

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create(dto) });

        // Act
        var result = await _monitor.GetRemoteSessionAsync(_id, _task, CancellationToken.None);

        // Assert
        result.Id.Value.Should().Be("sessions/session-123");
        result.RemoteId.Should().Be("rem-1");
        // High-Fidelity Update: We now expect the Remote Truth ("Original Prompt") to override the local fallback ("Fix bugs").
        result.Task.Value.Should().Be("Original Prompt");
        result.Pulse.Status.Should().Be(SessionStatus.Completed);
    }

    [Fact(DisplayName = "GetRemoteSessionAsync: Throws RemoteCollaboratorUnavailableException on non-success status code.")]
    public async Task GetRemoteSessionAsync_ThrowsOnHttpError()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.ServiceUnavailable, Content = new StringContent("Service Unavailable") });

        // Act
        var act = async () => await _monitor.GetRemoteSessionAsync(_id, _task, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RemoteCollaboratorUnavailableException>();
    }
}
