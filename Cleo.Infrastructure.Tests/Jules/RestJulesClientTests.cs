using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using FluentAssertions;
using Xunit;

namespace Cleo.Infrastructure.Tests.Jules;

public sealed class RestJulesClientTests : IDisposable
{
    private readonly JulesMockServer _julesMock = new();
    private readonly RestJulesClient _sut;

    public RestJulesClientTests()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri(_julesMock.Url) };
        _sut = new RestJulesClient(httpClient);
    }

    [Fact(DisplayName = "RestJulesClient should retrieve and map structured activities from the API.")]
    public async Task GetActivitiesAsync_ShouldReturnMappedActivities()
    {
        // Arrange
        const string sessionId = "cute-session-69";
        _julesMock.GivenActivitiesExist(sessionId);

        // Act
        var activities = await _sut.GetActivitiesAsync(
            new SessionId($"sessions/{sessionId}"), 
            TestContext.Current.CancellationToken);

        // Assert (This will FAIL with NotImplementedException!)
        activities.Should().NotBeEmpty();
        activities.Should().ContainSingle(a => a is PlanningActivity);
        
        var plan = activities.OfType<PlanningActivity>().Single();
        plan.Steps.Should().HaveCount(3);
        plan.Steps.First().Title.Should().Be("Analyze the dough density.");
    }

    public void Dispose()
    {
        _julesMock.Dispose();
    }
}
