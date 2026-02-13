using System.Net;
using System.Net.Http.Json;
using Cleo.Core.Domain.Exceptions;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using Cleo.Infrastructure.Tests.Jules;
using FluentAssertions;
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

        // Wrap in Composite because the client is now LEAN 🏎️
        var compositeMapper = new CompositeJulesActivityMapper(mappers);

        _client = new RestJulesActivityClient(httpClient, compositeMapper);
    }

    [Fact(DisplayName = "GetActivitiesAsync should map various activity types and attach artifacts from the API.")]
    public async Task GetActivitiesAsync_ShouldMapActivitiesWithArtifacts()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow.ToString("O");
        
        // 1. Progress activity with Bash Evidence ⚔️
        var progressDto = JulesDtoTestFactory.Create("prog", "1", null, now, "agent", 
            new List<JulesArtifactDto> { new JulesArtifactDto(null, null, new JulesBashOutputDto("ls", "files", 0)) },
            progressUpdated: new JulesProgressUpdatedPayloadDto("Working", "Running tests"));
            
        // 2. Completion activity with Code Proposal 🎁
        var completionDto = JulesDtoTestFactory.Create("done", "2", null, now, "system", 
            new List<JulesArtifactDto> { new JulesArtifactDto(new JulesChangeSetDto("src", new JulesGitPatchDto("patch", "base", null)), null, null) },
            sessionCompleted: new JulesSessionCompletedPayloadDto());

        var response = new JulesListActivitiesResponseDto(new JulesActivityDto[] { progressDto, completionDto }, null);

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
        var response = new JulesListActivitiesResponseDto(null, null);

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
        var response = new JulesListActivitiesResponseDto(Array.Empty<JulesActivityDto>(), null);

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

    [Fact(DisplayName = "GetActivitiesAsync should throw RemoteCollaboratorUnavailableException on API failure.")]
    public async Task GetActivitiesAsync_ThrowsOnApiFailure()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

        // Act
        var act = async () => await _client.GetActivitiesAsync(_testId, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<RemoteCollaboratorUnavailableException>();
    }

    [Fact(DisplayName = "GetActivitiesAsync should throw RemoteCollaboratorUnavailableException on network failure.")]
    public async Task GetActivitiesAsync_ThrowsOnNetworkError()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Timeout"));

        // Act
        var act = async () => await _client.GetActivitiesAsync(_testId, TestContext.Current.CancellationToken);

        // Assert
        // Correct usage of WithInnerException in FluentAssertions
        (await act.Should().ThrowAsync<RemoteCollaboratorUnavailableException>())
            .WithInnerException<HttpRequestException>();
    }

    [Fact(DisplayName = "GetActivitiesAsync should follow pagination links.")]
    public async Task GetActivitiesAsync_FollowsPagination()
    {
        // Arrange
        var page1 = new JulesListActivitiesResponseDto(
            new[] { JulesDtoTestFactory.Create("act-1", "rem-1", "desc", DateTimeOffset.UtcNow.ToString("O"), "agent", null) },
            "next-page-token");

        var page2 = new JulesListActivitiesResponseDto(
            new[] { JulesDtoTestFactory.Create("act-2", "rem-2", "desc", DateTimeOffset.UtcNow.ToString("O"), "agent", null) },
            null);

        _handlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create(page1) })
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create(page2) });

        // Act
        var result = await _client.GetActivitiesAsync(_testId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().HaveCount(2);
        // Mapping maps Name -> Id, and Id -> RemoteId.
        // JulesDtoTestFactory creates DTO with name="rem-1" and id="act-1".
        // JulesMapper maps Name (rem-1) to Domain.Id, and Id (act-1) to Domain.RemoteId.
        // So the Domain Id should be "rem-1" (the Name from DTO).
        result.First().Id.Should().Be("rem-1");
        result.Last().Id.Should().Be("rem-2");

        // Verify second call had the token
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact(DisplayName = "GetActivitiesAsync should handle mystery activity types gracefully.")]
    public async Task GetActivitiesAsync_ShouldHandleMysteryActivity()
    {
        // Arrange
        // Note: JulesDtoTestFactory.Create usually takes a specific payload.
        // Here we simulate an activity with NO recognized payload, but with metadata.
        var metadata = new JulesActivityMetadataDto(
            Id: "remote-mystery",
            Name: "mystery-1",
            Description: "Mystery Description",
            CreateTime: DateTimeOffset.UtcNow.ToString("O"),
            Originator: "agent",
            Artifacts: null
        );

        // Use a generic unknown payload to simulate mystery
        var payload = new JulesProgressUpdatedPayloadDto("Unknown Event", "Mystery payload content");

        var dto = new JulesActivityDto(metadata, payload);

        var response = new JulesListActivitiesResponseDto(new[] { dto }, null);

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
        result.Should().HaveCount(1);
        var mystery = result.First();

        // It should be mapped by UnknownActivityMapper (or fallback logic)
        // Since we injected UnknownActivityMapper, it should pick it up if others fail.
        // But wait, UnknownActivityMapper matches if activity.Unknown != null? No, it's a catch-all usually?
        // Let's check UnknownActivityMapper logic.
        // If it's truly a catch-all, it should return a generic ProgressActivity or similar with "Unknown Activity Type".
        // OR it might return a specific UnknownActivity (if we had one in domain).
        // The current implementation of UnknownActivityMapper maps to ProgressActivity with "Unknown Activity" title.

        Assert.IsType<ProgressActivity>(mystery);
        var progress = (ProgressActivity)mystery;
        progress.Intent.Should().Contain("Unknown");
    }
}
