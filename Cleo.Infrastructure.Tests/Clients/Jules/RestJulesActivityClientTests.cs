using System.Net;
using System.Net.Http.Json;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos;
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
            new ProgressActivityMapper(),
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
        // Order: Name, Id, CreateTime, Originator, PlanGenerated, PlanApproved, MessageText, Artifacts, ProgressUpdated, SessionFailed
        var planningDto = new JulesActivityDto("p", "1", DateTimeOffset.UtcNow, "agent", new PlanGeneratedDto(new PlanDto("pid", new[] { new PlanStepDto("s1", "Step 1", "Desc", 0) })), null, null, null, null, null);
        var messageDto = new JulesActivityDto("m", "2", DateTimeOffset.UtcNow, "agent", null, null, "Hello!", null, null, null);
        var resultDto = new JulesActivityDto("r", "3", DateTimeOffset.UtcNow, "agent", null, null, null, new[] { new ArtifactDto(new ChangeSetDto(new GitPatchDto("patch", "base"))) }, null, null);
        var failureDto = new JulesActivityDto("f", "4", DateTimeOffset.UtcNow, "agent", null, null, null, null, null, new SessionFailedDto("Broke!"));
        var progressDto = new JulesActivityDto("prog", "5", DateTimeOffset.UtcNow, "agent", null, null, null, null, new object(), null);
        var unknownDto = new JulesActivityDto("u", "6", DateTimeOffset.UtcNow, "agent", null, null, null, null, null, null);

        var response = new ListActivitiesResponse(new[] { planningDto, messageDto, resultDto, failureDto, progressDto, unknownDto }, null);

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
        Assert.Contains(result, a => a is ProgressActivity);
        Assert.Contains(result, a => a is MessageActivity m && m.Text.Contains("Unknown activity type"));
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
}
