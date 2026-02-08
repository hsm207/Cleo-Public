using System.Net;
using System.Net.Http.Json;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Moq;
using Moq.Protected;

namespace Cleo.Infrastructure.Tests.Clients.Jules;

public class RestJulesSourceClientTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock = new();
    private readonly RestJulesSourceClient _client;

    public RestJulesSourceClientTests()
    {
        var httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://jules.googleapis.com/")
        };
        _client = new RestJulesSourceClient(httpClient);
    }

    [Fact(DisplayName = "ListSourcesAsync should return a collection of sources from the API.")]
    public async Task ListSourcesAsync_ShouldReturnSources()
    {
        // Arrange
        var dto = new JulesSourceDto("sources/123", "123", new GithubRepoDto("owner", "repo"));
        var response = new ListSourcesResponse(new[] { dto }, null);

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(response)
            });

        // Act
        var result = await _client.ListSourcesAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        var source = result.First();
        Assert.Equal("sources/123", source.Name);
        Assert.Equal("owner", source.Owner);
        Assert.Equal("repo", source.Repo);
    }

    [Fact(DisplayName = "ListSourcesAsync should return unknown if githubRepo is null.")]
    public async Task ListSourcesAsync_ShouldHandleNullGithubRepo()
    {
        // Arrange
        var dto = new JulesSourceDto("sources/123", "123", null);
        var response = new ListSourcesResponse(new[] { dto }, null);

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(response)
            });

        // Act
        var result = await _client.ListSourcesAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        var source = result.First();
        Assert.Equal("unknown", source.Owner);
        Assert.Equal("unknown", source.Repo);
    }

    [Fact(DisplayName = "ListSourcesAsync should return an empty collection if the API returns null sources.")]
    public async Task ListSourcesAsync_ShouldReturnEmptyOnNull()
    {
        // Arrange
        var response = new ListSourcesResponse(null, null);

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(response)
            });

        // Act
        var result = await _client.ListSourcesAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact(DisplayName = "The client should correctly implement ISourceCatalog port.")]
    public async Task ShouldImplementPort()
    {
        // Arrange
        var catalog = (ISourceCatalog)_client;
        var response = new ListSourcesResponse(Array.Empty<JulesSourceDto>(), null);

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(response)
            });

        // Act
        var result = await catalog.GetAvailableSourcesAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }
}
