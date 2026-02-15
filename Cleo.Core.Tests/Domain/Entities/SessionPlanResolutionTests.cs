using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Events;
using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.Entities;

public class SessionPlanResolutionTests
{
    private static readonly SessionId Id = new("sessions/test-resolution");
    private const string RemoteId = "remote-123";
    private static readonly TaskDescription Task = (TaskDescription)"Resolution Test";
    private static readonly SourceContext Source = new("sources/repo", "main");
    private static readonly SessionPulse InitialPulse = new(SessionStatus.StartingUp);
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    private static Session CreateSession()
    {
        return new Session(Id, RemoteId, Task, Source, InitialPulse, Now);
    }

    [Fact(DisplayName = "Given a new session, when requesting the latest plan, then it should return null.")]
    public void FreshStartShouldReturnNull()
    {
        // Given
        var session = CreateSession();

        // When
        var plan = session.GetLatestPlan();

        // Then
        Assert.Null(plan);
    }

    [Fact(DisplayName = "Given a session with a single roadmap, when requesting the latest plan, then it should return that roadmap regardless of other activities.")]
    public void SingleRoadmapShouldReturnOriginal()
    {
        // Given
        var session = CreateSession();
        var plan = new PlanningActivity("plan-1", "remote-p1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "p1", Array.Empty<PlanStep>());

        session.AddActivity(new ProgressActivity("prog-1", "remote-prog-1", DateTimeOffset.UtcNow.AddMinutes(1), ActivityOriginator.Agent, "working..."));
        session.AddActivity(plan);
        session.AddActivity(new ProgressActivity("prog-2", "remote-prog-2", DateTimeOffset.UtcNow.AddMinutes(2), ActivityOriginator.Agent, "still working..."));

        // When
        var result = session.GetLatestPlan();

        // Then
        Assert.NotNull(result);
        Assert.Same(plan, result);
    }

    [Fact(DisplayName = "Given an evolving session with multiple roadmaps, when requesting the latest plan, then it should return the most recent roadmap by timestamp.")]
    public void EvolutionaryChangeShouldReturnLatest()
    {
        // Given
        var session = CreateSession();
        var now = DateTimeOffset.UtcNow;

        var planAlpha = new PlanningActivity("plan-alpha", "remote-alpha", now, ActivityOriginator.Agent, "alpha", Array.Empty<PlanStep>());
        var planBeta = new PlanningActivity("plan-beta", "remote-beta", now.AddHours(1), ActivityOriginator.Agent, "beta", Array.Empty<PlanStep>());

        session.AddActivity(planAlpha);
        session.AddActivity(new ProgressActivity("prog-1", "remote-prog-1", now.AddMinutes(30), ActivityOriginator.Agent, "working on alpha"));
        session.AddActivity(planBeta); // This is the latest one
        session.AddActivity(new ProgressActivity("prog-2", "remote-prog-2", now.AddHours(2), ActivityOriginator.Agent, "working on beta"));

        // When
        var result = session.GetLatestPlan();

        // Then
        Assert.NotNull(result);
        Assert.Same(planBeta, result);
        Assert.NotSame(planAlpha, result);
    }
}
