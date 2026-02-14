using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ViewPlan;
using FluentAssertions;
using Xunit;

namespace Cleo.Core.Tests.UseCases;

public class ViewPlanUseCaseTests
{
    private readonly FakeSessionReader _sessionReader = new();
    private readonly ViewPlanUseCase _useCase;

    public ViewPlanUseCaseTests()
    {
        _useCase = new ViewPlanUseCase(_sessionReader);
    }

    [Fact(DisplayName = "Given an unknown session, when viewing the plan, it should return an empty response.")]
    public async Task ShouldReturnEmptyWhenSessionNotFound()
    {
        var response = await _useCase.ExecuteAsync(new ViewPlanRequest(new SessionId("unknown")), TestContext.Current.CancellationToken);
        
        response.HasPlan.Should().BeFalse();
        response.Timestamp.Should().BeNull();
    }

    [Fact(DisplayName = "Given a session with no plan, when viewing the plan, it should return an empty response.")]
    public async Task ShouldReturnEmptyWhenNoPlan()
    {
        var sessionId = new SessionId("session-no-plan");
        var session = new Session(
            sessionId,
            "remote-1",
            new TaskDescription("Task"),
            new SourceContext("owner/repo", "main"),
            new SessionPulse(SessionStatus.StartingUp),
            DateTimeOffset.UtcNow
        );
        _sessionReader.Sessions[sessionId] = session;

        var response = await _useCase.ExecuteAsync(new ViewPlanRequest(sessionId), TestContext.Current.CancellationToken);

        response.HasPlan.Should().BeFalse();
        response.Timestamp.Should().BeNull();
    }

    [Fact(DisplayName = "Given a session with a plan, when viewing the plan, it should return the plan details.")]
    public async Task ShouldReturnPlanDetails()
    {
        var sessionId = new SessionId("session-with-plan");
        var now = DateTimeOffset.UtcNow;
        var session = new Session(
            sessionId,
            "remote-2",
            new TaskDescription("Task"),
            new SourceContext("owner/repo", "main"),
            new SessionPulse(SessionStatus.Planning),
            now
        );

        var steps = new List<PlanStep> { new("s1", 1, "Step 1", "Desc") };
        var planActivity = new PlanningActivity("act-1", "remote-act-1", now, ActivityOriginator.Agent, "PLAN-A", steps);
        session.AddActivity(planActivity);
        _sessionReader.Sessions[sessionId] = session;

        var response = await _useCase.ExecuteAsync(new ViewPlanRequest(sessionId), TestContext.Current.CancellationToken);

        response.HasPlan.Should().BeTrue();
        response.PlanId.Should().Be("PLAN-A");
        response.Timestamp.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
        response.Steps.Should().HaveCount(1);
        response.Steps[0].Title.Should().Be("Step 1");
    }

    private sealed class FakeSessionReader : ISessionReader
    {
        public Dictionary<SessionId, Session> Sessions { get; } = new();

        public Task<Session?> RecallAsync(SessionId id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Sessions.GetValueOrDefault(id));
        }

        public Task<IReadOnlyCollection<Session>> ListAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Session>>(Sessions.Values.ToList());
        }
    }
}
