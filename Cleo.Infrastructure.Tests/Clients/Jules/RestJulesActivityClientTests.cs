using System.Net;
using System.Net.Http.Json;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using Moq;
using Moq.Protected;

namespace Cleo.Infrastructure.Tests.Clients.Jules;

public class RestJulesActivityClientTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock = new();
    private readonly RestJulesActivityClient _client;
    private readonly SessionId _testId = new("sessions/123");

    public RestJulesActivityClientTests()
    {
        var httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://jules.googleapis.com/")
        };
        
        // REAL VIBES: Inject all real mappers
        var mappers = new IJulesActivityMapper[]
        {
            new PlanningActivityMapper(),
            new ResultActivityMapper(),
            new ExecutionActivityMapper(),
            new ProgressActivityMapper(),
            new CompletionActivityMapper(),
            new FailureActivityMapper(),
            new MessageActivityMapper(),
            new UnknownActivityMapper()
        };

        _client = new RestJulesActivityClient(httpClient, mappers);
    }

    [Fact(DisplayName = "GetActivitiesAsync should map various activity types from the API.")]
    public async Task GetActivitiesAsync_ShouldMapAllActivityTypes()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        
        var planningDto = new JulesActivityDto("p", "1", null, now, "agent", null, 
            new PlanGeneratedDto(new PlanDto("pid", new[] { new PlanStepDto("s1", "Step 1", "Desc", 0) }, now)), 
            null, null, null, null, null, null);
            
        var messageDto = new JulesActivityDto("m", "2", null, now, "user", null, null, null, 
            new UserMessagedDto("Hello!"), null, null, null, null);
            
        var resultDto = new JulesActivityDto("r", "3", null, now, "agent", 
            new[] { new ArtifactDto(new ChangeSetDto("src", new GitPatchDto("patch", "base", null)), null, null) }, 
            null, null, null, null, null, null, null);
            
        var failureDto = new JulesActivityDto("f", "4", null, now, "system", null, null, null, null, null, null, null, 
            new SessionFailedDto("Broke!"));
            
        var progressDto = new JulesActivityDto("prog", "5", null, now, "agent", null, null, null, null, null, 
            new ProgressUpdatedDto("Title", "Detail"), null, null);
            
        var execDto = new JulesActivityDto("exec", "6", null, now, "agent", 
            new[] { new ArtifactDto(null, null, new BashOutputDto("ls", "files", 0)) }, 
            null, null, null, null, null, null, null);

        var response = new ListActivitiesResponse(new[] { planningDto, messageDto, resultDto, failureDto, progressDto, execDto }, null);

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(response)
            });

        // Act
        var result = await _client.GetActivitiesAsync(_testId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(6, result.Count);
        Assert.Contains(result, a => a is PlanningActivity);
        Assert.Contains(result, a => a is MessageActivity m && m.Text == "Hello!");
        Assert.Contains(result, a => a is ResultActivity r && r.Patch.UniDiff == "patch");
        Assert.Contains(result, a => a is FailureActivity f && f.Reason == "Broke!");
        Assert.Contains(result, a => a is ProgressActivity p && p.Detail.Contains("Title"));
        Assert.Contains(result, a => a is ExecutionActivity e && e.Command == "ls");
    }

    [Fact(DisplayName = "GetActivitiesAsync should return an empty collection if the API returns null activities.")]
    public async Task GetActivitiesAsync_ShouldReturnEmptyOnNull()
    {
        // Arrange
        var response = new ListActivitiesResponse(null, null);

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(response)
            });

        // Act
        var result = await _client.GetActivitiesAsync(_testId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact(DisplayName = "The client should correctly implement ISessionArchivist port.")]
    public async Task ShouldImplementPort()
    {
        // Arrange
        var archivist = (ISessionArchivist)_client;
        var response = new ListActivitiesResponse(Array.Empty<JulesActivityDto>(), null);

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(response)
            });

        // Act
        var result = await archivist.GetHistoryAsync(_testId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }
}
