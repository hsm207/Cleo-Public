using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Events;
using Cleo.Core.Domain.Services;
using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.Entities;

public class SessionTests
{
    private static readonly SessionId Id = new("sessions/123");
    private const string RemoteId = "remote-123";
    private static readonly TaskDescription Task = (TaskDescription)"Fix login";
    private static readonly SourceContext Source = new("sources/repo", "main");
    private static readonly SessionPulse InitialPulse = new(SessionStatus.StartingUp, "Starting...");
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    private static Session CreateSession()
    {
        return new Session(Id, RemoteId, Task, Source, InitialPulse, Now);
    }

    [Fact(DisplayName = "Session should record 'SessionAssigned' when created.")]
    public void ConstructorShouldRecordEvent()
    {
        var session = CreateSession();

        var events = session.DomainEvents;
        Assert.Single(events);
        var sessionAssigned = Assert.IsType<SessionAssigned>(events.First());
        Assert.Equal(Id, sessionAssigned.SessionId);
        Assert.Equal(Task, sessionAssigned.Task);
        Assert.Equal(RemoteId, session.RemoteId);
        Assert.Equal(Now, session.CreatedAt);
    }

    [Fact(DisplayName = "Session should update pulse and record 'StatusHeartbeatReceived'.")]
    public void UpdatePulseShouldRecordEvent()
    {
        var session = CreateSession();
        var newPulse = new SessionPulse(SessionStatus.InProgress, "Working hard!");

        session.UpdatePulse(newPulse);

        Assert.Equal(newPulse, session.Pulse);
        var events = session.DomainEvents;
        Assert.Contains(events, e => e is StatusHeartbeatReceived);
    }

    [Fact(DisplayName = "Session should record 'FeedbackRequested' when pulse status is 'AwaitingFeedback'.")]
    public void UpdatePulseShouldSignalFeedbackRequest()
    {
        var session = CreateSession();
        var feedbackPulse = new SessionPulse(SessionStatus.AwaitingFeedback, "What color should the button be?");

        session.UpdatePulse(feedbackPulse);

        var events = session.DomainEvents;
        Assert.Contains(events, e => e is FeedbackRequested);
    }

    [Fact(DisplayName = "Session should add activities to the Session Log.")]
    public void AddActivityShouldUpdateLog()
    {
        var session = CreateSession();
        var activity = new MessageActivity("act/1", "remote-act-1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Hello!");

        session.AddActivity(activity);

        Assert.Single(session.SessionLog);
        Assert.Equal(activity, session.SessionLog.First());
    }

    [Fact(DisplayName = "Session should update the solution and record 'SolutionReady' when an activity with a ChangeSet is added.")]
    public void AddingChangeSetEvidenceShouldUpdateSolution()
    {
        var session = CreateSession();
        var patch = new GitPatch("diff-content", "base-commit");
        var changeSet = new ChangeSet("sources/repo", patch);
        var evidence = new List<Artifact> { changeSet };
        var activity = new ProgressActivity("act/2", "remote-act-2", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Done", null, evidence);

        session.AddActivity(activity);

        Assert.Equal(changeSet, session.Solution);
        var events = session.DomainEvents;
        Assert.Contains(events, e => e is SolutionReady);
    }

    [Fact(DisplayName = "Session should allow adding user feedback via convenience method.")]
    public void AddFeedbackShouldCreateMessageActivity()
    {
        var session = CreateSession();
        
        session.AddFeedback("This looks great!", "act/3");

        var activity = Assert.IsType<MessageActivity>(session.SessionLog.First());
        Assert.Equal(ActivityOriginator.User, activity.Originator);
        Assert.Equal("This looks great!", activity.Text);
    }

    [Fact(DisplayName = "Session should record FeedbackRequested with detail when pulse status is AwaitingFeedback.")]
    public void UpdatePulseShouldRecordFeedbackDetail()
    {
        var session = CreateSession();
        var detail = "Which file should I edit?";
        var feedbackPulse = new SessionPulse(SessionStatus.AwaitingFeedback, detail);

        session.UpdatePulse(feedbackPulse);

        var feedbackEvent = session.DomainEvents.OfType<FeedbackRequested>().Single();
        Assert.Equal(detail, feedbackEvent.Prompt);
    }

    [Fact(DisplayName = "Session should allow clearing domain events.")]
    public void ClearDomainEventsShouldWork()
    {
        var session = CreateSession();
        Assert.NotEmpty(session.DomainEvents);

        session.ClearDomainEvents();

        Assert.Empty(session.DomainEvents);
    }

    [Fact(DisplayName = "Domain Events should support explicit timestamps.")]
    public void EventsShouldSupportExplicitTimestamps()
    {
        var now = DateTimeOffset.UtcNow.AddDays(-1);
        var e1 = new SessionAssigned(Id, Task, now);
        var e2 = new StatusHeartbeatReceived(Id, InitialPulse, now);
        var e3 = new FeedbackRequested(Id, "prompt", now);
        var e4 = new SolutionReady(Id, new ChangeSet("s", new GitPatch("d", "b")), now);

        Assert.Equal(now, e1.OccurredOn);
        Assert.Equal(now, e2.OccurredOn);
        Assert.Equal(now, e3.OccurredOn);
        Assert.Equal(now, e4.OccurredOn);
    }

    [Fact(DisplayName = "Session should record all activity types in the log.")]
    public void AddActivityShouldSupportAllTypes()
    {
        var session = CreateSession();
        var now = DateTimeOffset.UtcNow;

        session.AddActivity(new PlanningActivity("a1", "r1", now, ActivityOriginator.Agent, "plan-1", new[] { new PlanStep("step-1", 0, "T", "D") }));
        session.AddActivity(new MessageActivity("a2", "r2", now, ActivityOriginator.User, "msg"));
        session.AddActivity(new ProgressActivity("a3", "r3", now, ActivityOriginator.Agent, "working"));
        session.AddActivity(new FailureActivity("a4", "r4", now, ActivityOriginator.System, "error"));

        Assert.Equal(4, session.SessionLog.Count);
    }

    [Fact(DisplayName = "Session should update pulse and record events for all statuses.")]
    public void UpdatePulseShouldHandleAllStatuses()
    {
        var session = CreateSession();
        
        // Test normal transition
        session.UpdatePulse(new SessionPulse(SessionStatus.InProgress, "Detail"));
        Assert.Equal(SessionStatus.InProgress, session.Pulse.Status);

        // Test terminal failure
        session.UpdatePulse(new SessionPulse(SessionStatus.Failed, "Crash"));
        Assert.Equal(SessionStatus.Failed, session.Pulse.Status);

        // Test completion
        session.UpdatePulse(new SessionPulse(SessionStatus.Completed, "Done"));
        Assert.Equal(SessionStatus.Completed, session.Pulse.Status);
    }

    [Fact(DisplayName = "Session should handle all possible status heartbeats.")]
    public void UpdatePulseShouldExerciseAllStatuses()
    {
        var session = CreateSession();
        
        foreach (SessionStatus status in Enum.GetValues<SessionStatus>())
        {
            var pulse = new SessionPulse(status, $"Pulse for {status}");
            session.UpdatePulse(pulse);
            Assert.Equal(status, session.Pulse.Status);
        }
    }

    [Fact(DisplayName = "Domain Events should have all properties hit by the coverage tool.")]
    public void EventsShouldBeFullyExercised()
    {
        var now = DateTimeOffset.UtcNow;
        var patch = new GitPatch("d", "b");
        var changeSet = new ChangeSet("s", patch);
        
        var e1 = new SessionAssigned(Id, Task, now);
        var e2 = new StatusHeartbeatReceived(Id, InitialPulse, now);
        var e3 = new FeedbackRequested(Id, "prompt", now);
        var e4 = new SolutionReady(Id, changeSet, now);

        // Explicitly hit EVERY property of EVERY event
        Assert.Equal(Id, e1.SessionId);
        Assert.Equal(Task, e1.Task);
        Assert.Equal(now, e1.OccurredOn);

        Assert.Equal(Id, e2.SessionId);
        Assert.Equal(InitialPulse, e2.Pulse);
        Assert.Equal(now, e2.OccurredOn);

        Assert.Equal(Id, e3.SessionId);
        Assert.Equal("prompt", e3.Prompt);
        Assert.Equal(now, e3.OccurredOn);

        Assert.Equal(Id, e4.SessionId);
        Assert.Equal(changeSet, e4.Solution);
        Assert.Equal(now, e4.OccurredOn);
    }

    [Fact(DisplayName = "Session should expose all properties correctly.")]
    public void PropertiesShouldBeReadable()
    {
        // Use full constructor to hit all properties
        var session = new Session(
            Id,
            RemoteId,
            Task,
            Source,
            InitialPulse,
            Now,
            Now.AddMinutes(5),
            "My Title",
            true,
            AutomationMode.AutoCreatePr,
            new Uri("https://dashboard.com"));

        Assert.Equal(Id, session.Id);
        Assert.Equal(RemoteId, session.RemoteId);
        Assert.Equal(Task, session.Task);
        Assert.Equal(Source, session.Source);
        Assert.Equal(InitialPulse, session.Pulse);
        Assert.Equal(Now, session.CreatedAt);
        Assert.Equal(Now.AddMinutes(5), session.UpdatedAt);
        Assert.Equal("My Title", session.Title);
        Assert.True(session.RequiresPlanApproval);
        Assert.Equal(AutomationMode.AutoCreatePr, session.Mode);
        Assert.Equal(new Uri("https://dashboard.com"), session.DashboardUri);

        Assert.Null(session.Solution);
        Assert.Empty(session.SessionLog);
    }

    [Fact(DisplayName = "Session should validate primitive arguments.")]
    public void MethodsShouldValidateArgs()
    {
        var session = CreateSession();
        
        // Validation for primitives (strings) remains.
        Assert.Throws<ArgumentNullException>(() => session.AddFeedback(null!, "id"));
        Assert.Throws<ArgumentException>(() => session.AddFeedback(" ", "id"));
    }

    [Fact(DisplayName = "Domain Events should expose all properties correctly.")]
    public void EventsShouldExposeProperties()
    {
        var now = DateTimeOffset.UtcNow;
        var patch = new GitPatch("d", "b");
        var changeSet = new ChangeSet("s", patch);
        
        var e1 = new SessionAssigned(Id, Task);
        var e2 = new StatusHeartbeatReceived(Id, InitialPulse);
        var e3 = new FeedbackRequested(Id, "prompt");
        var e4 = new SolutionReady(Id, changeSet);

        Assert.Equal(Id, e1.SessionId);
        Assert.Equal(Task, e1.Task);
        Assert.Equal(InitialPulse, e2.Pulse);
        Assert.Equal("prompt", e3.Prompt);
        Assert.Equal(changeSet, e4.Solution);
        
        // Exercise the 'OccurredOn' from secondary constructor
        Assert.True((DateTimeOffset.UtcNow - e1.OccurredOn).TotalSeconds < 5);
    }

    [Fact(DisplayName = "Session should validate primitive constructor arguments.")]
    public void ConstructorShouldValidateArgs()
    {
        // Validation for primitives (strings) remains.
        Assert.Throws<ArgumentNullException>(() => new Session(Id, null!, Task, Source, InitialPulse, Now));
    }

    [Fact(DisplayName = "GetSignificantHistory should exclude non-significant activities.")]
    public void GetSignificantHistoryShouldFilter()
    {
        var session = CreateSession();
        var now = DateTimeOffset.UtcNow;

        session.AddActivity(new PlanningActivity("a1", "r1", now, ActivityOriginator.Agent, "plan-1", new[] { new PlanStep("s1", 0, "T", "D") }));
        session.AddActivity(new ProgressActivity("a2", "r2", now, ActivityOriginator.Agent, "working")); // Not significant
        session.AddActivity(new MessageActivity("a3", "r3", now, ActivityOriginator.User, "msg"));
        session.AddActivity(new ProgressActivity("a4", "r4", now, ActivityOriginator.Agent, "working more")); // Not significant
        session.AddActivity(new CompletionActivity("a5", "r5", now, ActivityOriginator.System));

        var significantHistory = session.GetSignificantHistory();

        Assert.Equal(3, significantHistory.Count);
        Assert.Contains(significantHistory, a => a.Id == "a1");
        Assert.Contains(significantHistory, a => a.Id == "a3");
        Assert.Contains(significantHistory, a => a.Id == "a5");
        Assert.DoesNotContain(significantHistory, a => a.Id == "a2");
        Assert.DoesNotContain(significantHistory, a => a.Id == "a4");
    }

    [Fact(DisplayName = "EvaluatedStance should override to AwaitingPlanApproval when Idle, no PR, and last significant activity was Planning.")]
    public void EvaluatedStanceShouldOverrideToAwaitingPlanApproval()
    {
        var session = CreateSession();
        // 1. Last significant activity is Planning
        session.AddActivity(new PlanningActivity("a1", "r1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Plan", new[] { new PlanStep("1", 0, "T", "D") }));

        // 2. Pulse is Idle (Completed)
        session.UpdatePulse(new SessionPulse(SessionStatus.Completed, "Done"));

        // 3. No PR set

        // Assert override logic
        Assert.Equal(Stance.AwaitingPlanApproval, session.EvaluatedStance);
    }

    [Fact(DisplayName = "EvaluatedStance should NOT override if PR is present.")]
    public void EvaluatedStanceShouldNotOverrideIfPrPresent()
    {
        var session = CreateSession();
        session.AddActivity(new PlanningActivity("a1", "r1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Plan", new[] { new PlanStep("1", 0, "T", "D") }));
        session.UpdatePulse(new SessionPulse(SessionStatus.Completed, "Done"));
        session.SetPullRequest(new PullRequest(new Uri("https://github.com/pr/1"), "PR"));

        // Should be Idle because PR is delivered
        Assert.Equal(Stance.Idle, session.EvaluatedStance);
    }

    [Fact(DisplayName = "DeliveryStatus should be Delivered if Solution or PR is present.")]
    public void DeliveryStatusShouldBeDeliveredIfArtifactsPresent()
    {
        var session = CreateSession();

        // Case 1: Solution present
        var patch = new GitPatch("d", "b");
        var changeSet = new ChangeSet("s", patch);
        var evidence = new List<Artifact> { changeSet };
        session.AddActivity(new ProgressActivity("a1", "r1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Done", null, evidence));

        Assert.Equal(DeliveryStatus.Delivered, session.DeliveryStatus);

        // Case 2: PR present
        var session2 = CreateSession();
        session2.SetPullRequest(new PullRequest(new Uri("https://github.com/pr/1"), "PR"));
        Assert.Equal(DeliveryStatus.Delivered, session2.DeliveryStatus);
    }

    [Fact(DisplayName = "DeliveryStatus should be Unfulfilled if Idle and not Delivered.")]
    public void DeliveryStatusShouldBeUnfulfilledIfIdleAndNotDelivered()
    {
        var session = CreateSession();
        session.UpdatePulse(new SessionPulse(SessionStatus.Completed, "Done")); // Idle

        Assert.Equal(DeliveryStatus.Unfulfilled, session.DeliveryStatus);
    }

    [Fact(DisplayName = "DeliveryStatus should be Stalled if Broken.")]
    public void DeliveryStatusShouldBeStalledIfBroken()
    {
        var session = CreateSession();
        session.UpdatePulse(new SessionPulse(SessionStatus.Failed, "Boom"));

        Assert.Equal(DeliveryStatus.Stalled, session.DeliveryStatus);
    }

    [Fact(DisplayName = "DeliveryStatus should be Pending if Working.")]
    public void DeliveryStatusShouldBePendingIfWorking()
    {
        var session = CreateSession();
        session.UpdatePulse(new SessionPulse(SessionStatus.InProgress, "Working"));

        Assert.Equal(DeliveryStatus.Pending, session.DeliveryStatus);
    }

    [Fact(DisplayName = "SetPullRequest should update property.")]
    public void SetPullRequestShouldWork()
    {
        var session = CreateSession();
        var pr = new PullRequest(new Uri("https://github.com/pr/1"), "PR");

        session.SetPullRequest(pr);

        Assert.Equal(pr, session.PullRequest);
    }

    [Fact(DisplayName = "GetLatestPlan should resolve using provided strategy.")]
    public void GetLatestPlanShouldUseStrategy()
    {
        var session = CreateSession();
        var plan = new PlanningActivity("a1", "r1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Plan", new[] { new PlanStep("1", 0, "T", "D") });
        session.AddActivity(plan);

        var retrievedPlan = session.GetLatestPlan(); // Default strategy
        Assert.Equal(plan, retrievedPlan);

        // Mock strategy
        var strategy = new MockPlanStrategy(plan);
        var retrievedPlan2 = session.GetLatestPlan(strategy);
        Assert.Equal(plan, retrievedPlan2);
    }

    private sealed class MockPlanStrategy : IPlanResolutionStrategy
    {
        private readonly PlanningActivity _plan;
        public MockPlanStrategy(PlanningActivity plan) => _plan = plan;
        public PlanningActivity? ResolvePlan(IEnumerable<SessionActivity> history) => _plan;
    }

    [Fact(DisplayName = "EvaluatedStance should map all statuses correctly.")]
    public void EvaluatedStanceShouldMapAllStatuses()
    {
        var session = CreateSession();

        // Verify key mappings
        session.UpdatePulse(new SessionPulse(SessionStatus.StartingUp, ""));
        Assert.Equal(Stance.Queued, session.EvaluatedStance);

        session.UpdatePulse(new SessionPulse(SessionStatus.Planning, ""));
        Assert.Equal(Stance.Planning, session.EvaluatedStance);

        session.UpdatePulse(new SessionPulse(SessionStatus.AwaitingPlanApproval, ""));
        Assert.Equal(Stance.AwaitingPlanApproval, session.EvaluatedStance);

        session.UpdatePulse(new SessionPulse(SessionStatus.Abandoned, ""));
        Assert.Equal(Stance.Idle, session.EvaluatedStance);

        session.UpdatePulse(new SessionPulse(SessionStatus.Paused, ""));
        Assert.Equal(Stance.Paused, session.EvaluatedStance);

        session.UpdatePulse(new SessionPulse(SessionStatus.AwaitingFeedback, ""));
        Assert.Equal(Stance.AwaitingFeedback, session.EvaluatedStance);
    }

    [Fact(DisplayName = "EvaluatedStance should map 'StateUnspecified' and unknown values to 'WTF'.")]
    public void EvaluatedStanceShouldMapUnknownToWtf()
    {
        var session = CreateSession();

        // StateUnspecified -> WTF 🚨
        session.UpdatePulse(new SessionPulse(SessionStatus.StateUnspecified, "Unknown"));
        Assert.Equal(Stance.WTF, session.EvaluatedStance);

        // Unknown Int -> WTF 🚨
        var invalidStatus = (SessionStatus)999;
        session.UpdatePulse(new SessionPulse(invalidStatus, "Invalid"));
        Assert.Equal(Stance.WTF, session.EvaluatedStance);
    }
}
