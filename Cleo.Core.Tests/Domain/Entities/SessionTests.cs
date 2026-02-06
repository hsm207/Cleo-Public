using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.Entities;

public class SessionTests
{
    private readonly SessionId _testId = new("sessions/123");
    private readonly TaskDescription _testTask = new("Fix bug");
    private readonly SourceContext _testSource = new("repo", "main");
    private readonly SessionPulse _testPulse = new(SessionStatus.StartingUp);

    [Fact(DisplayName = "A Session should be correctly initialized with valid identity, task, source, and pulse.")]
    public void ConstructorShouldInitializeCorrectly()
    {
        var session = new Session(_testId, _testTask, _testSource, _testPulse);

        Assert.Equal(_testId, session.Id);
        Assert.Equal(_testTask, session.Task);
        Assert.Equal(_testSource, session.Source);
        Assert.Equal(_testPulse, session.Pulse);
        Assert.Empty(session.Conversation);
        Assert.Null(session.Solution);
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

    [Fact(DisplayName = "A Session's Pulse should be updatable to reflect progress.")]
    public void UpdatePulseShouldUpdateStatus()
    {
        var session = new Session(_testId, _testTask, _testSource, _testPulse);
        var nextPulse = new SessionPulse(SessionStatus.InProgress, "Thinking...");

        session.UpdatePulse(nextPulse);

        Assert.Equal(nextPulse, session.Pulse);
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

    [Fact(DisplayName = "A Session should capture the final SolutionPatch when it is produced.")]
    public void SetSolutionShouldUpdateSolution()
    {
        var session = new Session(_testId, _testTask, _testSource, _testPulse);
        var patch = new SolutionPatch("diff", "sha");

        session.SetSolution(patch);

        Assert.Equal(patch, session.Solution);
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
