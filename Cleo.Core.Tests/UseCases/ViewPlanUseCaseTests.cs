using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ViewPlan;
using Cleo.Tests.Common;
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
        var response = await _useCase.ExecuteAsync(new ViewPlanRequest(TestFactory.CreateSessionId("unknown")), TestContext.Current.CancellationToken);
        
        response.HasPlan.Should().BeFalse();
        response.Timestamp.Should().BeNull();
    }

    [Fact(DisplayName = "Given a session with no plan, when viewing the plan, it should return an empty response.")]
    public async Task ShouldReturnEmptyWhenNoPlan()
    {
        var sessionId = TestFactory.CreateSessionId("session-no-plan");
        var session = new Session(
            sessionId,
            "remote-1",
            new TaskDescription("Task"),
            TestFactory.CreateSourceContext("owner/repo"),
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
        var sessionId = TestFactory.CreateSessionId("session-with-plan");
        var now = DateTimeOffset.UtcNow;
        var session = new Session(
            sessionId,
            "remote-2",
            new TaskDescription("Task"),
            TestFactory.CreateSourceContext("owner/repo"),
            new SessionPulse(SessionStatus.Planning),
            now
        );

        var steps = new List<PlanStep> { new("s1", 1, "Step 1", "Desc") };
        var planActivity = new PlanningActivity("act-1", "remote-act-1", now, ActivityOriginator.Agent, TestFactory.CreatePlanId("PLAN-A"), steps);
        session.AddActivity(planActivity);
        _sessionReader.Sessions[sessionId] = session;

        var response = await _useCase.ExecuteAsync(new ViewPlanRequest(sessionId), TestContext.Current.CancellationToken);

        response.HasPlan.Should().BeTrue();
        response.PlanId.Should().Be(TestFactory.CreatePlanId("PLAN-A"));
        response.Timestamp.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
        response.Steps.Should().HaveCount(1);
        response.Steps[0].Title.Should().Be("Step 1");
    }

    [Fact(DisplayName = "Given a session awaiting plan approval, when viewing the plan, IsApproved should be false.")]
    public async Task ShouldReturnFalseForIsApprovedWhenAwaitingApproval()
    {
        var sessionId = TestFactory.CreateSessionId("session-awaiting-approval");
        var now = DateTimeOffset.UtcNow;
        var session = new Session(
            sessionId,
            "remote-3",
            new TaskDescription("Task"),
            TestFactory.CreateSourceContext("owner/repo"),
            new SessionPulse(SessionStatus.AwaitingPlanApproval),
            now
        );

        var steps = new List<PlanStep> { new("s1", 1, "Step 1", "Desc") };
        var planActivity = new PlanningActivity("act-1", "remote-act-1", now, ActivityOriginator.Agent, TestFactory.CreatePlanId("PLAN-B"), steps);
        session.AddActivity(planActivity);
        _sessionReader.Sessions[sessionId] = session;

        var response = await _useCase.ExecuteAsync(new ViewPlanRequest(sessionId), TestContext.Current.CancellationToken);

        response.HasPlan.Should().BeTrue();
        response.IsApproved.Should().BeFalse();
    }

    [Fact(DisplayName = "Given a session NOT awaiting plan approval, when viewing the plan, IsApproved should be true.")]
    public async Task ShouldReturnTrueForIsApprovedWhenWorking()
    {
        var sessionId = TestFactory.CreateSessionId("session-working");
        var now = DateTimeOffset.UtcNow;
        var session = new Session(
            sessionId,
            "remote-4",
            new TaskDescription("Task"),
            TestFactory.CreateSourceContext("owner/repo"),
            new SessionPulse(SessionStatus.InProgress),
            now
        );

        var steps = new List<PlanStep> { new("s1", 1, "Step 1", "Desc") };
        var planActivity = new PlanningActivity("act-1", "remote-act-1", now, ActivityOriginator.Agent, TestFactory.CreatePlanId("PLAN-C"), steps);
        session.AddActivity(planActivity);
        _sessionReader.Sessions[sessionId] = session;

        var response = await _useCase.ExecuteAsync(new ViewPlanRequest(sessionId), TestContext.Current.CancellationToken);

        response.HasPlan.Should().BeTrue();
        response.IsApproved.Should().BeTrue();
    }

    [Fact(DisplayName = "Given a plan step with a multi-line description, when viewing the plan, the description formatting should be preserved.")]
    public async Task ShouldPreserveMultiLineDescriptionFormatting()
    {
        var sessionId = TestFactory.CreateSessionId("session-multiline");
        var now = DateTimeOffset.UtcNow;
        var session = new Session(
            sessionId,
            "remote-5",
            new TaskDescription("Task"),
            TestFactory.CreateSourceContext("owner/repo"),
            new SessionPulse(SessionStatus.Planning),
            now
        );

        var description = "Line 1\nLine 2\r\nLine 3";
        var steps = new List<PlanStep> { new("s1", 1, "Step 1", description) };
        var planActivity = new PlanningActivity("act-1", "remote-act-1", now, ActivityOriginator.Agent, TestFactory.CreatePlanId("PLAN-D"), steps);
        session.AddActivity(planActivity);
        _sessionReader.Sessions[sessionId] = session;

        var response = await _useCase.ExecuteAsync(new ViewPlanRequest(sessionId), TestContext.Current.CancellationToken);

        response.HasPlan.Should().BeTrue();
        response.Steps.Should().HaveCount(1);
        response.Steps[0].Description.Should().Be(description);
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
