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

public class RestSessionLifecycleClientTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock = new();
    private readonly ISessionStatusMapper _statusMapper = new DefaultSessionStatusMapper();
    private readonly RestSessionLifecycleClient _client;

    public RestSessionLifecycleClientTests()
    {
        var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("https://jules.googleapis.com/") };
        _client = new RestSessionLifecycleClient(httpClient, _statusMapper);
    }

    [Fact(DisplayName = "CreateSession: Enforces OCP by mapping Domain Objects to a rigid Request DTO.")]
    public async Task CreateSession_EnforcesStrictContract()
    {
        // Arrange
        var task = new TaskDescription("Refactor the world");
        var source = new SourceContext("sources/cleo-repo", "main");
        var options = new SessionCreationOptions(AutomationMode.AutoCreatePr, "Refactor Session", true);
        
        // The mock response
        var responseDto = new JulesSessionResponseDto(
            Name: "sessions/session-1",
            Id: "id",
            State: JulesSessionState.Queued,
            Prompt: "Refactor the world",
            SourceContext: new JulesSourceContextDto("org", new JulesGithubRepoContextDto("sources/repo")),
            Url: null,
            RequirePlanApproval: true,
            AutomationMode: JulesAutomationMode.AutoCreatePr,
            CreateTime: null,
            UpdateTime: null,
            Title: null,
            Outputs: null
        );
        
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create(responseDto) });

        // Act
        var result = await _client.CreateSessionAsync(task, source, options, CancellationToken.None);

        // Assert - Verify the Result
        Assert.Equal("sessions/session-1", result.Id.Value);

        // Assert - Verify the Contract (The "Spec" part)
        // We ensure the client sent the EXACT structure defined in our new Request DTOs.
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Method == HttpMethod.Post && 
                req.RequestUri!.ToString() == "https://jules.googleapis.com/v1alpha/sessions" &&
                VerifyRequestBody(req, "Refactor the world", "AUTO_CREATE_PR") // Check specific JSON fields
            ),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact(DisplayName = "CreateSession: Throws RemoteCollaboratorUnavailableException on API failure.")]
    public async Task CreateSession_ThrowsOnApiFailure()
    {
        // Arrange
        var task = new TaskDescription("Task");
        var source = new SourceContext("sources/repo", "main");
        var options = new SessionCreationOptions(AutomationMode.Unspecified, "Title", false);

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

        // Act
        var act = async () => await _client.CreateSessionAsync(task, source, options, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RemoteCollaboratorUnavailableException>();
    }

    [Fact(DisplayName = "CreateSession: Throws RemoteCollaboratorUnavailableException on network error.")]
    public async Task CreateSession_ThrowsOnNetworkError()
    {
        // Arrange
        var task = new TaskDescription("Task");
        var source = new SourceContext("sources/repo", "main");
        var options = new SessionCreationOptions(AutomationMode.Unspecified, "Title", false);

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var act = async () => await _client.CreateSessionAsync(task, source, options, CancellationToken.None);

        // Assert
        // Correct usage of WithInnerException in FluentAssertions
        (await act.Should().ThrowAsync<RemoteCollaboratorUnavailableException>())
            .WithInnerException<HttpRequestException>();
    }

    private bool VerifyRequestBody(HttpRequestMessage req, string expectedPrompt, string expectedMode)
    {
        var json = req.Content!.ReadAsStringAsync().Result;
        // This assertion proves we aren't using anonymous objects anymore. 
        // We expect specific serialized field names from 'JulesCreateSessionRequestDto'.
        return json.Contains($"\"prompt\":\"{expectedPrompt}\"") && 
               json.Contains($"\"automationMode\":\"{expectedMode}\"");
    }
}
