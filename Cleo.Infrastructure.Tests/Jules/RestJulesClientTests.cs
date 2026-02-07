using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos;
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

    [Fact(DisplayName = "RestJulesClient should retrieve the current session pulse.")]
    public async Task GetSessionPulseAsync_ShouldReturnMappedPulse()
    {
        // Arrange
        const string sessionId = "pulse-session-123";
        _julesMock.GivenSessionPulseExists(sessionId, "IN_PROGRESS");

        // Act
        var pulse = await _sut.GetSessionPulseAsync(
            new SessionId($"sessions/{sessionId}"), 
            TestContext.Current.CancellationToken);

        // Assert
        pulse.Status.Should().Be(SessionStatus.InProgress);
    }

    [Fact(DisplayName = "RestJulesClient should send feedback messages to the API.")]
    public async Task SendMessageAsync_ShouldPostCorrectPayload()
    {
        // Arrange
        const string sessionId = "msg-session-456";
        _julesMock.GivenMessageCanBeSent(sessionId);

        // Act & Assert
        Func<Task> act = () => _sut.SendMessageAsync(
            new SessionId($"sessions/{sessionId}"), 
            "Great job, Jules!", 
            TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "RestJulesClient should correctly map and exercise all DTO properties.")]
    public async Task Client_ShouldExerciseAllProperties()
    {
        // Arrange
        const string sessionId = "cute-session-69";
        _julesMock.GivenSessionIsCreated();
        _julesMock.GivenActivitiesExist(sessionId);
        _julesMock.GivenSessionPulseExists(sessionId, "STARTING_UP");

        // Act
        var session = await _sut.CreateSessionAsync((TaskDescription)"t", new SourceContext("r", "b"), TestContext.Current.CancellationToken);
        var pulse = await _sut.GetSessionPulseAsync(session.Id, TestContext.Current.CancellationToken);
        var activities = await _sut.GetActivitiesAsync(session.Id, TestContext.Current.CancellationToken);

        // Assert & Exercise (Touch EVERY property!)
        session.Id.Value.Should().Be("sessions/cute-session-69");
        session.Task.Should().Be((TaskDescription)"t");
        session.Source.Repository.Should().Be("sources/github/cleo-lover/cookie-bakery");
        session.Source.StartingBranch.Should().Be("main");
        
        pulse.Status.Should().Be(SessionStatus.StartingUp);
        pulse.Detail.Should().Contain("STARTING_UP");

        activities.Should().NotBeEmpty();
        var planning = activities.OfType<PlanningActivity>().Single();
        planning.Id.Should().NotBeNull();
        planning.Timestamp.Should().NotBe(default);
        planning.Originator.Should().Be(ActivityOriginator.Agent);
        planning.Steps.First().Index.Should().Be(0);
        planning.Steps.First().Description.Should().NotBeNull();
        
        var msg = activities.OfType<MessageActivity>().Single();
        msg.Text.Should().Contain("delicious");
        
        var result = activities.OfType<ResultActivity>().Single();
        result.Patch.BaseCommitId.Should().Be("base-cookie-commit-001");
        
        // Exercise the default status mapping
        JulesMapper.MapStatus("UNKNOWN_STATE").Should().Be(SessionStatus.InProgress);
    }

    [Fact(DisplayName = "RestJulesClient should validate all arguments and exercise all properties.")]
    public async Task Methods_ShouldTriggerAllValidations()
    {
        var ct = TestContext.Current.CancellationToken;
        // 1. Trigger all null checks in SUT
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateSessionAsync(null!, new SourceContext("r", "b"), ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateSessionAsync((TaskDescription)"t", null!, ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetSessionPulseAsync(null!, ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.SendMessageAsync(null!, "f", ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.SendMessageAsync(new SessionId("s"), null!, ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetActivitiesAsync(null!, ct));

        // 2. Trigger empty string check (Explicitly hit both null and whitespace branches!)
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.SendMessageAsync(new SessionId("s"), " ", ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.SendMessageAsync(new SessionId("s"), null!, ct));
    }

    [Fact(DisplayName = "RestJulesClient should handle error responses gracefully.")]
    public async Task Client_ShouldHandleErrors()
    {
        _julesMock.GivenUnauthenticated();
        
        await _sut.Invoking(s => s.GetActivitiesAsync(new SessionId("sessions/123"), TestContext.Current.CancellationToken))
            .Should().ThrowAsync<HttpRequestException>();
    }

    [Fact(DisplayName = "JulesMapper should map unknown status strings to InProgress.")]
    public void MapStatus_ShouldHandleUnknownStates()
    {
        JulesMapper.MapStatus("WHATEVER").Should().Be(SessionStatus.InProgress);
    }

    [Fact(DisplayName = "RestJulesClient should throw when pulse response is null.")]
    public async Task GetSessionPulseAsync_ShouldThrowOnNullResponse()
    {
        // Arrange
        const string sessionId = "null-session";
        _julesMock.GivenSessionPulseReturnsNull(sessionId);

        // Act
        Func<Task> act = () => _sut.GetSessionPulseAsync(
            new SessionId($"sessions/{sessionId}"), 
            TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to retrieve session pulse*");
    }

    [Fact(DisplayName = "JulesMapper should throw on unknown activity patterns.")]
    public void Map_ShouldThrowOnUnknownPattern()
    {
        var dto = new JulesActivityDto("n", "i", DateTimeOffset.UtcNow, "agent", null, null, null, null, null, null);
        
        Action act = () => JulesMapper.Map(dto);
        act.Should().Throw<InvalidOperationException>().WithMessage("*No suitable mapping pattern*");
    }

    [Fact(DisplayName = "RestJulesClient should return empty list when API returns no activities.")]
    public async Task GetActivitiesAsync_ShouldReturnEmpty_WhenResponseHasNoActivities()
    {
        // Arrange
        const string sessionId = "empty-session";
        // Configure mock to return 200 OK but with a body that has no 'activities' array (e.g., just {})
        // Since our mock server helper assumes valid JSON, we'll use a custom setup here for this edge case.
        // Or simpler: add a helper to the mock server. Let's add 'GivenActivitiesAreEmpty'.
        _julesMock.GivenActivitiesAreEmpty(sessionId);

        // Act
        var result = await _sut.GetActivitiesAsync(
            new SessionId($"sessions/{sessionId}"), 
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEmpty();
    }

    public void Dispose()
    {
        _julesMock.Dispose();
    }
}
