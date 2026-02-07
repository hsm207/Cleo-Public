using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Events;
using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.Entities;

public class SessionTests
{
    private static readonly SessionId Id = new("sessions/123");
    private static readonly TaskDescription Task = (TaskDescription)"Fix login";
    private static readonly SourceContext Source = new("sources/repo", "main");
    private static readonly SessionPulse InitialPulse = new(SessionStatus.StartingUp, "Starting...");

    [Fact(DisplayName = "Session should record 'SessionAssigned' when created.")]
    public void ConstructorShouldRecordEvent()
    {
        var session = new Session(Id, Task, Source, InitialPulse);

        var events = session.DomainEvents;
        Assert.Single(events);
        var sessionAssigned = Assert.IsType<SessionAssigned>(events.First());
        Assert.Equal(Id, sessionAssigned.SessionId);
        Assert.Equal(Task, sessionAssigned.Task);
    }

    [Fact(DisplayName = "Session should update pulse and record 'StatusHeartbeatReceived'.")]
    public void UpdatePulseShouldRecordEvent()
    {
        var session = new Session(Id, Task, Source, InitialPulse);
        var newPulse = new SessionPulse(SessionStatus.InProgress, "Working hard!");

        session.UpdatePulse(newPulse);

        Assert.Equal(newPulse, session.Pulse);
        var events = session.DomainEvents;
        Assert.Contains(events, e => e is StatusHeartbeatReceived);
    }

    [Fact(DisplayName = "Session should record 'FeedbackRequested' when pulse status is 'AwaitingFeedback'.")]
    public void UpdatePulseShouldSignalFeedbackRequest()
    {
        var session = new Session(Id, Task, Source, InitialPulse);
        var feedbackPulse = new SessionPulse(SessionStatus.AwaitingFeedback, "What color should the button be?");

        session.UpdatePulse(feedbackPulse);

        var events = session.DomainEvents;
        Assert.Contains(events, e => e is FeedbackRequested);
    }

    [Fact(DisplayName = "Session should add activities to the Session Log.")]
    public void AddActivityShouldUpdateLog()
    {
        var session = new Session(Id, Task, Source, InitialPulse);
        var activity = new MessageActivity("act/1", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Hello!");

        session.AddActivity(activity);

        Assert.Single(session.SessionLog);
        Assert.Equal(activity, session.SessionLog.First());
    }

    [Fact(DisplayName = "Session should update the solution and record 'SolutionReady' when a ResultActivity is added.")]
    public void AddResultActivityShouldUpdateSolution()
    {
        var session = new Session(Id, Task, Source, InitialPulse);
        var patch = new SolutionPatch("diff-content", "base-commit");
        var activity = new ResultActivity("act/2", DateTimeOffset.UtcNow, patch);

        session.AddActivity(activity);

        Assert.Equal(patch, session.Solution);
        var events = session.DomainEvents;
        Assert.Contains(events, e => e is SolutionReady);
    }

    [Fact(DisplayName = "Session should allow adding user feedback via convenience method.")]
    public void AddFeedbackShouldCreateMessageActivity()
    {
        var session = new Session(Id, Task, Source, InitialPulse);
        
        session.AddFeedback("This looks great!", "act/3");

        var activity = Assert.IsType<MessageActivity>(session.SessionLog.First());
        Assert.Equal(ActivityOriginator.User, activity.Originator);
        Assert.Equal("This looks great!", activity.Text);
    }

    [Fact(DisplayName = "Session should record FeedbackRequested with detail when pulse status is AwaitingFeedback.")]
    public void UpdatePulseShouldRecordFeedbackDetail()
    {
        var session = new Session(Id, Task, Source, InitialPulse);
        var detail = "Which file should I edit?";
        var feedbackPulse = new SessionPulse(SessionStatus.AwaitingFeedback, detail);

        session.UpdatePulse(feedbackPulse);

        var feedbackEvent = session.DomainEvents.OfType<FeedbackRequested>().Single();
        Assert.Equal(detail, feedbackEvent.Prompt);
    }

    [Fact(DisplayName = "Session should allow clearing domain events.")]
    public void ClearDomainEventsShouldWork()
    {
        var session = new Session(Id, Task, Source, InitialPulse);
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
        var e4 = new SolutionReady(Id, new SolutionPatch("d", "b"), now);

        Assert.Equal(now, e1.OccurredOn);
        Assert.Equal(now, e2.OccurredOn);
        Assert.Equal(now, e3.OccurredOn);
        Assert.Equal(now, e4.OccurredOn);
    }

    [Fact(DisplayName = "Session should record all activity types in the log.")]
    public void AddActivityShouldSupportAllTypes()
    {
        var session = new Session(Id, Task, Source, InitialPulse);
        var now = DateTimeOffset.UtcNow;

        session.AddActivity(new PlanningActivity("a1", now, "plan-1", new[] { new PlanStep(0, "T", "D") }));
        session.AddActivity(new ExecutionActivity("a2", now, "cmd", "out", 0));
        session.AddActivity(new ProgressActivity("a3", now, "working"));
        session.AddActivity(new FailureActivity("a4", now, "error"));

        Assert.Equal(4, session.SessionLog.Count);
    }

    [Fact(DisplayName = "Session should update pulse and record events for all statuses.")]
    public void UpdatePulseShouldHandleAllStatuses()
    {
        var session = new Session(Id, Task, Source, InitialPulse);
        
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
        var session = new Session(Id, Task, Source, InitialPulse);
        
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
        var patch = new SolutionPatch("d", "b");
        
        var e1 = new SessionAssigned(Id, Task, now);
        var e2 = new StatusHeartbeatReceived(Id, InitialPulse, now);
        var e3 = new FeedbackRequested(Id, "prompt", now);
        var e4 = new SolutionReady(Id, patch, now);

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
        Assert.Equal(patch, e4.Solution);
        Assert.Equal(now, e4.OccurredOn);
    }

    [Fact(DisplayName = "Session should expose all properties correctly.")]
    public void PropertiesShouldBeReadable()
    {
        var session = new Session(Id, Task, Source, InitialPulse);
        Assert.Equal(Id, session.Id);
        Assert.Equal(Task, session.Task);
        Assert.Equal(Source, session.Source);
        Assert.Equal(InitialPulse, session.Pulse);
        Assert.Null(session.Solution);
        Assert.Empty(session.SessionLog);
    }

    [Fact(DisplayName = "Session should throw ArgumentNullException if method arguments are null.")]
    public void MethodsShouldValidateArgs()
    {
        var session = new Session(Id, Task, Source, InitialPulse);
        
        Assert.Throws<ArgumentNullException>(() => session.UpdatePulse(null!));
        Assert.Throws<ArgumentNullException>(() => session.AddActivity(null!));
        Assert.Throws<ArgumentNullException>(() => session.AddFeedback(null!, "id"));
        Assert.Throws<ArgumentException>(() => session.AddFeedback(" ", "id"));
    }

    [Fact(DisplayName = "Domain Events should expose all properties correctly.")]
    public void EventsShouldExposeProperties()
    {
        var now = DateTimeOffset.UtcNow;
        var patch = new SolutionPatch("d", "b");
        
        var e1 = new SessionAssigned(Id, Task);
        var e2 = new StatusHeartbeatReceived(Id, InitialPulse);
        var e3 = new FeedbackRequested(Id, "prompt");
        var e4 = new SolutionReady(Id, patch);

        Assert.Equal(Id, e1.SessionId);
        Assert.Equal(Task, e1.Task);
        Assert.Equal(InitialPulse, e2.Pulse);
        Assert.Equal("prompt", e3.Prompt);
        Assert.Equal(patch, e4.Solution);
        
        // Exercise the 'OccurredOn' from secondary constructor
        Assert.True((DateTimeOffset.UtcNow - e1.OccurredOn).TotalSeconds < 5);
    }

    [Fact(DisplayName = "Session should throw ArgumentNullException if constructor arguments are null.")]
    public void ConstructorShouldValidateArgs()
    {
        Assert.Throws<ArgumentNullException>(() => new Session(null!, Task, Source, InitialPulse));
        Assert.Throws<ArgumentNullException>(() => new Session(Id, null!, Source, InitialPulse));
        Assert.Throws<ArgumentNullException>(() => new Session(Id, Task, null!, InitialPulse));
        Assert.Throws<ArgumentNullException>(() => new Session(Id, Task, Source, null!));
    }
}
