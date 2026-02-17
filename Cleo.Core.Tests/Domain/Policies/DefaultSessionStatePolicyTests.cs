using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Policies;
using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.Policies;

public class DefaultSessionStatePolicyTests
{
    private readonly DefaultSessionStatePolicy _policy = new();

    [Fact(DisplayName = "Evaluate should return AwaitingPlanApproval when Idle, Not Delivered, and Last Significant Activity is Planning.")]
    public void EvaluateShouldOverrideToAwaitingPlanApproval()
    {
        // Arrange
        var pulse = new SessionPulse(SessionStatus.Completed); // Idle
        var history = new List<SessionActivity>
        {
            new SessionAssignedActivity("id1", "rem1", DateTimeOffset.UtcNow, ActivityOriginator.System, (TaskDescription)"Task"),
            new PlanningActivity("id2", "rem2", DateTimeOffset.UtcNow.AddMinutes(1), ActivityOriginator.Agent, new PlanId("plans/Plan"), new[] { new PlanStep("1", 0, "T", "D") })
        };
        var isDelivered = false;

        // Act
        var state = _policy.Evaluate(pulse, history, isDelivered);

        // Assert
        Assert.Equal(SessionState.AwaitingPlanApproval, state);
    }

    [Fact(DisplayName = "Evaluate should NOT override if IsDelivered is true.")]
    public void EvaluateShouldNotOverrideIfDelivered()
    {
        // Arrange
        var pulse = new SessionPulse(SessionStatus.Completed); // Idle
        var history = new List<SessionActivity>
        {
            new SessionAssignedActivity("id1", "rem1", DateTimeOffset.UtcNow, ActivityOriginator.System, (TaskDescription)"Task"),
            new PlanningActivity("id2", "rem2", DateTimeOffset.UtcNow.AddMinutes(1), ActivityOriginator.Agent, new PlanId("plans/Plan"), new[] { new PlanStep("1", 0, "T", "D") })
        };
        var isDelivered = true;

        // Act
        var state = _policy.Evaluate(pulse, history, isDelivered);

        // Assert
        Assert.Equal(SessionState.Idle, state);
    }

    [Theory(DisplayName = "Evaluate should map physical status to expected session state.")]
    [InlineData(SessionStatus.StartingUp, SessionState.Queued)]
    [InlineData(SessionStatus.Planning, SessionState.Planning)]
    [InlineData(SessionStatus.InProgress, SessionState.Working)]
    [InlineData(SessionStatus.Paused, SessionState.Paused)]
    [InlineData(SessionStatus.AwaitingFeedback, SessionState.AwaitingFeedback)]
    [InlineData(SessionStatus.AwaitingPlanApproval, SessionState.AwaitingPlanApproval)]
    [InlineData(SessionStatus.Completed, SessionState.Idle)]
    [InlineData(SessionStatus.Abandoned, SessionState.Idle)]
    [InlineData(SessionStatus.Failed, SessionState.Broken)]
    [InlineData((SessionStatus)999, SessionState.WTF)]
    public void EvaluateShouldMapStatusToState(SessionStatus status, SessionState expectedState)
    {
        // Arrange
        var pulse = new SessionPulse(status);
        var history = new List<SessionActivity>
        {
            new SessionAssignedActivity("id1", "rem1", DateTimeOffset.UtcNow, ActivityOriginator.System, (TaskDescription)"Task")
        };
        var isDelivered = false;

        // Act
        var state = _policy.Evaluate(pulse, history, isDelivered);

        // Assert
        Assert.Equal(expectedState, state);
    }

    [Fact(DisplayName = "Evaluate should handle empty history gracefully (though Zero-Hollow forbids it).")]
    public void EvaluateShouldHandleEmptyHistory()
    {
        // Arrange
        var pulse = new SessionPulse(SessionStatus.Completed);
        var history = new List<SessionActivity>();
        var isDelivered = false;

        // Act
        var state = _policy.Evaluate(pulse, history, isDelivered);

        // Assert
        Assert.Equal(SessionState.Idle, state);
    }

    [Fact(DisplayName = "Evaluate should throw if arguments are null.")]
    public void EvaluateShouldThrowIfArgsNull()
    {
        var history = new List<SessionActivity>();
        var pulse = new SessionPulse(SessionStatus.Completed);

        Assert.Throws<ArgumentNullException>(() => _policy.Evaluate(null!, history, false));
        Assert.Throws<ArgumentNullException>(() => _policy.Evaluate(pulse, null!, false));
    }
}
