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
    private readonly Mock<IJulesActivityMapper> _mapperMock = new();
    private readonly RestJulesActivityClient _client;
    private readonly SessionId _testId = new("sessions/123");

    public RestJulesActivityClientTests()
    {
        var httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://jules.googleapis.com/")
        };
        _client = new RestJulesActivityClient(httpClient, new[] { _mapperMock.Object });
    }

    [Fact(DisplayName = "GetActivitiesAsync should return a collection of activities from the API.")]
    public async Task GetActivitiesAsync_ShouldReturnActivities()
    {
        // Arrange
        var dto = new JulesActivityDto("name", "id", DateTimeOffset.UtcNow, "agent", null, null, "text", null, null, null);
        var response = new ListActivitiesResponse(new[] { dto }, null);
        var mappedActivity = new MessageActivity("id", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "text");

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(response)
            });

        _mapperMock.Setup(m => m.CanMap(dto)).Returns(true);
        _mapperMock.Setup(m => m.Map(dto)).Returns(mappedActivity);

        // Act
        var result = await _client.GetActivitiesAsync(_testId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal(mappedActivity, result.First());
    }

    [Fact(DisplayName = "GetActivitiesAsync should return a fallback message if no mapper can handle the activity.")]
    public async Task GetActivitiesAsync_ShouldReturnFallbackOnUnmappable()
    {
        // Arrange
        var dto = new JulesActivityDto("weird-name", "id", DateTimeOffset.UtcNow, "unknown", null, null, null, null, null, null);
        var response = new ListActivitiesResponse(new[] { dto }, null);

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(response)
            });

        _mapperMock.Setup(m => m.CanMap(dto)).Returns(false);

        // Act
        var result = await _client.GetActivitiesAsync(_testId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        var activity = Assert.IsType<MessageActivity>(result.First());
        Assert.Contains("Unknown activity type", activity.Text);
        Assert.Equal(ActivityOriginator.System, activity.Originator);
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
