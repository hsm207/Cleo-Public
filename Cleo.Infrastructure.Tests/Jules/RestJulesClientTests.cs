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

    [Fact(DisplayName = "RestJulesClient should launch a new session and map it to a Domain Session entity.")]
    public async Task CreateSessionAsync_ShouldReturnMappedSession()
    {
        // Arrange
        _julesMock.GivenSessionIsCreated();
        var task = (TaskDescription)"Optimize cookies";
        var source = new SourceContext("sources/github/repo", "main");

        // Act
        var session = await _sut.CreateSessionAsync(task, source, TestContext.Current.CancellationToken);

        // Assert
        session.Id.Value.Should().Be("sessions/cute-session-69");
        session.Task.Should().Be(task);
        session.Source.Repository.Should().Be("sources/github/cleo-lover/cookie-bakery");
        session.Pulse.Status.Should().Be(SessionStatus.StartingUp);
    }

    public void Dispose()
    {
        _julesMock.Dispose();
    }
}
