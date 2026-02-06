using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Events;
using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.Entities;

public class SessionTests
{
    private readonly SessionId _testId = new("sessions/123");
    private readonly TaskDescription _testTask = new("Fix bug");
    private readonly SourceContext _testSource = new("repo", "main");
    private readonly SessionPulse _testPulse = new(SessionStatus.StartingUp);

    [Fact(DisplayName = "A Session should be correctly initialized and raise a SessionAssigned event.")]
    public void ConstructorShouldInitializeCorrectly()
    {
        var session = new Session(_testId, _testTask, _testSource, _testPulse);

        Assert.Equal(_testId, session.Id);
        Assert.Equal(_testTask, session.Task);
        Assert.Equal(_testSource, session.Source);
        Assert.Equal(_testPulse, session.Pulse);
        Assert.Empty(session.Conversation);
        Assert.Null(session.Solution);

        // Verify Domain Event
        Assert.Single(session.DomainEvents);
        var evt = Assert.Single(session.DomainEvents.OfType<SessionAssigned>());
        Assert.Equal(_testId, evt.SessionId);
        Assert.Equal(_testTask, evt.Task);
    }

    [Fact(DisplayName = "A Session should allow clearing its uncommitted domain events.")]
    public void ClearDomainEventsShouldEmptyCollection()
    {
        var session = new Session(_testId, _testTask, _testSource, _testPulse);
        Assert.NotEmpty(session.DomainEvents);

        session.ClearDomainEvents();

        Assert.Empty(session.DomainEvents);
    }

    [Theory(DisplayName = "A Session should throw ArgumentNullException if any required initialization component is missing.")]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(false, false, false, true)]
    public void ConstructorShouldEnforceTransactionalIntegrity(bool nullId, bool nullTask, bool nullSource, bool nullPulse)
    {
        Assert.Throws<ArgumentNullException>(() => new Session(
            nullId ? null! : _testId,
            nullTask ? null! : _testTask,
            nullSource ? null! : _testSource,
            nullPulse ? null! : _testPulse));
    }

    [Fact(DisplayName = "A Session's Pulse update should raise a StatusHeartbeatReceived event.")]
    public void UpdatePulseShouldRaiseHeartbeatEvent()
    {
        var session = new Session(_testId, _testTask, _testSource, _testPulse);
        session.ClearDomainEvents();
        var nextPulse = new SessionPulse(SessionStatus.InProgress, "Thinking...");

        session.UpdatePulse(nextPulse);

        Assert.Equal(nextPulse, session.Pulse);
        var evt = Assert.Single(session.DomainEvents.OfType<StatusHeartbeatReceived>());
        Assert.Equal(_testId, evt.SessionId);
        Assert.Equal(nextPulse, evt.Pulse);
    }

    [Fact(DisplayName = "Transitioning to AwaitingFeedback should raise a FeedbackRequested event.")]
    public void AwaitingFeedbackStatusShouldRaiseFeedbackEvent()
    {
        var session = new Session(_testId, _testTask, _testSource, _testPulse);
        session.ClearDomainEvents();
        var feedbackPulse = new SessionPulse(SessionStatus.AwaitingFeedback, "What do?");

        session.UpdatePulse(feedbackPulse);

        var events = session.DomainEvents.ToList();
        Assert.Contains(events, e => e is StatusHeartbeatReceived);
        var feedbackEvt = Assert.Single(events.OfType<FeedbackRequested>());
        Assert.Equal(_testId, feedbackEvt.SessionId);
        Assert.Equal("What do?", feedbackEvt.Prompt);
    }

    [Fact(DisplayName = "A Session's Conversation should be append-only and maintain chronological order.")]
    public void AddMessageShouldAppendToConversation()
    {
        var session = new Session(_testId, _testTask, _testSource, _testPulse);
        var msg1 = new ChatMessage(MessageSender.User, "Hello", DateTimeOffset.UtcNow);
        var msg2 = new ChatMessage(MessageSender.Agent, "Hi", DateTimeOffset.UtcNow.AddSeconds(1));

        session.AddMessage(msg1);
        session.AddMessage(msg2);

        Assert.Equal(2, session.Conversation.Count);
        Assert.Collection(session.Conversation,
            m => Assert.Equal(msg1, m),
            m => Assert.Equal(msg2, m));
    }

    [Fact(DisplayName = "A Session should raise a SolutionReady event when a final patch is set.")]
    public void SetSolutionShouldRaiseSolutionReadyEvent()
    {
        var session = new Session(_testId, _testTask, _testSource, _testPulse);
        session.ClearDomainEvents();
        var patch = new SolutionPatch("diff", "sha");

        session.SetSolution(patch);

        Assert.Equal(patch, session.Solution);
        var evt = Assert.Single(session.DomainEvents.OfType<SolutionReady>());
        Assert.Equal(_testId, evt.SessionId);
        Assert.Equal(patch, evt.Solution);
    }

    [Fact(DisplayName = "A Session should reject null inputs for pulse updates, messages, or solutions.")]
    public void MethodsShouldThrowOnNull()
    {
        var session = new Session(_testId, _testTask, _testSource, _testPulse);

        Assert.Throws<ArgumentNullException>(() => session.UpdatePulse(null!));
        Assert.Throws<ArgumentNullException>(() => session.AddMessage(null!));
        Assert.Throws<ArgumentNullException>(() => session.SetSolution(null!));
    }
}
