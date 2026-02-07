using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos;
using Cleo.Infrastructure.Clients.Jules.Mapping;
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
        var mappers = new IJulesActivityMapper[]
        {
            new PlanningActivityMapper(),
            new ResultActivityMapper(),
            new ProgressActivityMapper(),
            new FailureActivityMapper(),
            new MessageActivityMapper()
        };
        _sut = new RestJulesClient(httpClient, mappers);
    }

    [Fact(DisplayName = "RestJulesClient should retrieve and map structured activities from the API.")]
    public async Task GetActivitiesAsync_ShouldReturnMappedActivities()
    {
        const string sessionId = "cute-session-69";
        _julesMock.GivenActivitiesExist(sessionId);

        var activities = await _sut.GetActivitiesAsync(
            new SessionId($"sessions/{sessionId}"), 
            TestContext.Current.CancellationToken);

        activities.Should().NotBeEmpty();
        activities.Should().ContainSingle(a => a is PlanningActivity);
        
        var plan = activities.OfType<PlanningActivity>().Single();
        plan.Steps.Should().HaveCount(3);
        plan.Steps.First().Title.Should().Be("Analyze the dough density.");
    }

    [Fact(DisplayName = "RestJulesClient should launch a new session and map it to a Domain Session entity.")]
    public async Task CreateSessionAsync_ShouldReturnMappedSession()
    {
        _julesMock.GivenSessionIsCreated();
        var task = (TaskDescription)"Optimize cookies";
        var source = new SourceContext("sources/github/repo", "main");

        var session = await _sut.CreateSessionAsync(task, source, TestContext.Current.CancellationToken);

        session.Id.Value.Should().Be("sessions/cute-session-69");
        session.Task.Should().Be(task);
        session.Source.Repository.Should().Be("sources/github/cleo-lover/cookie-bakery");
        session.Pulse.Status.Should().Be(SessionStatus.StartingUp);
    }

    [Fact(DisplayName = "RestJulesClient should retrieve the current session pulse.")]
    public async Task GetSessionPulseAsync_ShouldReturnMappedPulse()
    {
        const string sessionId = "pulse-session-123";
        _julesMock.GivenSessionPulseExists(sessionId, "IN_PROGRESS");

        var pulse = await _sut.GetSessionPulseAsync(
            new SessionId($"sessions/{sessionId}"), 
            TestContext.Current.CancellationToken);

        pulse.Status.Should().Be(SessionStatus.InProgress);
    }

    [Fact(DisplayName = "RestJulesClient should send feedback messages to the API.")]
    public async Task SendMessageAsync_ShouldPostCorrectPayload()
    {
        const string sessionId = "msg-session-456";
        _julesMock.GivenMessageCanBeSent(sessionId);

        Func<Task> act = () => _sut.SendMessageAsync(
            new SessionId($"sessions/{sessionId}"), 
            "Great job, Jules!", 
            TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "RestJulesClient should correctly map and exercise all DTO properties.")]
    public async Task Client_ShouldExerciseAllProperties()
    {
        const string sessionId = "cute-session-69";
        _julesMock.GivenSessionIsCreated();
        _julesMock.GivenActivitiesExist(sessionId);
        _julesMock.GivenSessionPulseExists(sessionId, "STARTING_UP");

        var session = await _sut.CreateSessionAsync((TaskDescription)"t", new SourceContext("r", "b"), TestContext.Current.CancellationToken);
        var pulse = await _sut.GetSessionPulseAsync(session.Id, TestContext.Current.CancellationToken);
        var activities = await _sut.GetActivitiesAsync(session.Id, TestContext.Current.CancellationToken);

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
        
        JulesMapper.MapStatus("UNKNOWN_STATE").Should().Be(SessionStatus.InProgress);
        var dto = new JulesSessionDto("n", "i", "STARTING_UP", "p", new SourceContextDto("r", new GithubRepoContextDto("main")));
        JulesMapper.Map(dto, (TaskDescription)"t").Should().NotBeNull();
    }

    [Fact(DisplayName = "RestJulesClient should validate all arguments and exercise all properties.")]
    public async Task Methods_ShouldTriggerAllValidations()
    {
        var ct = TestContext.Current.CancellationToken;
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateSessionAsync(null!, new SourceContext("r", "b"), ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateSessionAsync((TaskDescription)"t", null!, ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetSessionPulseAsync(null!, ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.SendMessageAsync(null!, "f", ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.SendMessageAsync(new SessionId("s"), null!, ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetActivitiesAsync(null!, ct));
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.SendMessageAsync(new SessionId("s"), " ", ct));
    }

    [Fact(DisplayName = "RestJulesClient should handle error responses gracefully.")]
    public async Task Client_ShouldHandleErrors()
    {
        _julesMock.GivenUnauthenticated();
        await _sut.Invoking(s => s.GetActivitiesAsync(new SessionId("sessions/123"), TestContext.Current.CancellationToken))
            .Should().ThrowAsync<HttpRequestException>();
    }

    [Fact(DisplayName = "RestJulesClient should throw when pulse response is null.")]
    public async Task GetSessionPulseAsync_ShouldThrowOnNullResponse()
    {
        const string sessionId = "null-session";
        _julesMock.GivenSessionPulseReturnsNull(sessionId);

        Func<Task> act = () => _sut.GetSessionPulseAsync(
            new SessionId($"sessions/{sessionId}"), 
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to retrieve session pulse*");
    }

    [Fact(DisplayName = "RestJulesClient should return empty list when API returns no activities.")]
    public async Task GetActivitiesAsync_ShouldReturnEmpty_WhenResponseHasNoActivities()
    {
        const string sessionId = "empty-session";
        _julesMock.GivenActivitiesAreEmpty(sessionId);

        var result = await _sut.GetActivitiesAsync(
            new SessionId($"sessions/{sessionId}"), 
            TestContext.Current.CancellationToken);

        result.Should().BeEmpty();
    }

    public void Dispose()
    {
        _julesMock.Dispose();
    }
}
