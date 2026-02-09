using System.Net;
using System.Net.Http.Json;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using Moq;
using Moq.Protected;
using Xunit;

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
        
        // Inject modern high-fidelity mappers 🔌🏺
        var mappers = new IJulesActivityMapper[]
        {
            new PlanningActivityMapper(),
            new ApprovalActivityMapper(),
            new ProgressActivityMapper(),
            new CompletionActivityMapper(),
            new FailureActivityMapper(),
            new MessageActivityMapper(),
            new UnknownActivityMapper()
        };

        _client = new RestJulesActivityClient(httpClient, mappers);
    }

    [Fact(DisplayName = "GetActivitiesAsync should map various activity types and attach artifacts from the API.")]
    public async Task GetActivitiesAsync_ShouldMapActivitiesWithArtifacts()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        
        // 1. Progress activity with Bash Evidence ⚔️
        var progressDto = new JulesActivityDto("prog", "1", null, now, "agent", 
            new[] { new ArtifactDto(null, null, new BashOutputDto("ls", "files", 0)) }, 
            null, null, null, null, new ProgressUpdatedDto("Working", "Running tests"), null, null);
            
        // 2. Completion activity with Code Proposal 🎁
        var completionDto = new JulesActivityDto("done", "2", null, now, "system", 
            new[] { new ArtifactDto(new ChangeSetDto("src", new GitPatchDto("patch", "base", null)), null, null) }, 
            null, null, null, null, null, new SessionCompletedDto(), null);

        var response = new ListActivitiesResponse(new[] { progressDto, completionDto }, null);

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
        Assert.Equal(2, result.Count);
        
        var progress = Assert.IsType<ProgressActivity>(result.First());
        Assert.Single(progress.Evidence);
        Assert.IsType<BashOutput>(progress.Evidence.First());
        
        var completion = Assert.IsType<CompletionActivity>(result.Last());
        Assert.Single(completion.Evidence);
        Assert.IsType<ChangeSet>(completion.Evidence.First());
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
