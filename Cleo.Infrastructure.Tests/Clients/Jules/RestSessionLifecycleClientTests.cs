using System.Net;
using System.Net.Http.Json;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Mapping;
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
        var source = new SourceContext("cleo-repo", "main");
        var options = new SessionCreationOptions(AutomationMode.AutoCreatePr, "Refactor Session", true);
        
        // The mock response
        var responseDto = new JulesSessionResponseDto("session-1", "id", JulesSessionState.Queued, "Refactor the world", new JulesSourceContextDto("repo", new JulesGithubRepoContextDto("main")), null, true, JulesAutomationMode.AutoCreatePr, null, null);
        
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create(responseDto) });

        // Act
        var result = await _client.CreateSessionAsync(task, source, options, CancellationToken.None);

        // Assert - Verify the Result
        Assert.Equal("session-1", result.Id.Value);

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

    private bool VerifyRequestBody(HttpRequestMessage req, string expectedPrompt, string expectedMode)
    {
        var json = req.Content!.ReadAsStringAsync().Result;
        // This assertion proves we aren't using anonymous objects anymore. 
        // We expect specific serialized field names from 'JulesCreateSessionRequestDto'.
        return json.Contains($"\"prompt\":\"{expectedPrompt}\"") && 
               json.Contains($"\"automationMode\":\"{expectedMode}\"");
    }
}
