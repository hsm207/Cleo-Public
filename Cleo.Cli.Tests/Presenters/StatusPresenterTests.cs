using Cleo.Cli.Presenters;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.RefreshPulse;
using FluentAssertions;
using Xunit;

namespace Cleo.Cli.Tests.Presenters;

public class StatusPresenterTests
{
    [Fact(DisplayName = "Given null response, it should throw ArgumentNullException")]
    public void ShouldThrowArgumentNullException_WhenResponseIsNull()
    {
        // Act
        Action act = () => StatusPresenter.Format(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Given Paused State, it should show 'Paused'")]
    public void ShouldShowPaused()
    {
        // Arrange
        var response = CreateResponse(SessionState.Paused, null);

        // Act
        var output = StatusPresenter.Format(response);

        // Assert
        output.Should().Contain("🧘‍♀️ Session State: [Paused]");
    }

    [Fact(DisplayName = "Given In-Progress Session (No PR), it should show 'In Progress'")]
    public void ShouldShowInProgress_WhenWorkingNoPR()
    {
        // Arrange
        var response = CreateResponse(SessionState.Working, null);

        // Act
        var output = StatusPresenter.Format(response);

        // Assert
        output.Should().Contain("🧘‍♀️ Session State: [Working]");
        output.Should().Contain("🎁 Pull Request: ⏳ In Progress");
    }

    [Fact(DisplayName = "Given Active Iteration (PR Exists), it should show 'Iterating | URL'")]
    public void ShouldShowIterating_WhenWorkingWithPR()
    {
        // Arrange
        var pr = new PullRequest(new Uri("https://github.com/pr/1"), "PR");
        var response = CreateResponse(SessionState.Working, pr);

        // Act
        var output = StatusPresenter.Format(response);

        // Assert
        output.Should().Contain("🎁 Pull Request: 🔄 Iterating | https://github.com/pr/1");
    }

    [Fact(DisplayName = "Given Collaborative Blocker (PR Exists), it should show 'Awaiting Response | URL'")]
    public void ShouldShowAwaitingResponse_WhenAwaitingFeedbackWithPR()
    {
        // Arrange
        var pr = new PullRequest(new Uri("https://github.com/pr/1"), "PR");
        var response = CreateResponse(SessionState.AwaitingFeedback, pr);

        // Act
        var output = StatusPresenter.Format(response);

        // Assert
        output.Should().Contain("🎁 Pull Request: ⏳ Awaiting your response... | https://github.com/pr/1");
    }

    [Fact(DisplayName = "Given Success State (PR Exists), it should show 'Check | URL'")]
    public void ShouldShowSuccess_WhenIdleWithPR()
    {
        // Arrange
        var pr = new PullRequest(new Uri("https://github.com/pr/1"), "PR");
        var response = CreateResponse(SessionState.Idle, pr);

        // Act
        var output = StatusPresenter.Format(response);

        // Assert
        output.Should().Contain("🧘‍♀️ Session State: [Finished]"); // Assuming Idle -> Finished mapping
        output.Should().Contain("🎁 Pull Request: ✅ https://github.com/pr/1");
    }

    [Fact(DisplayName = "Given Abnormal Completion (WTF), it should show 'WTF?!'")]
    public void ShouldShowWTF_WhenIdleNoPR()
    {
        // Arrange
        var response = CreateResponse(SessionState.Idle, null);

        // Act
        var output = StatusPresenter.Format(response);

        // Assert
        output.Should().Contain("🎁 Pull Request: WTF?! 🤪 (Finished with no PR)");
    }

    [Fact(DisplayName = "Given Stalled State, it should show 'Stalled'")]
    public void ShouldShowStalled_WhenBroken()
    {
        // Arrange
        var response = CreateResponse(SessionState.Broken, null);

        // Act
        var output = StatusPresenter.Format(response);

        // Assert
        output.Should().Contain("🎁 Pull Request: 🛑 Stalled");
    }

    [Fact(DisplayName = "Given Interrupted State, it should show 'Stalled'")]
    public void ShouldShowStalled_WhenInterrupted()
    {
        // Arrange
        var response = CreateResponse(SessionState.Interrupted, null);

        // Act
        var output = StatusPresenter.Format(response);

        // Assert
        output.Should().Contain("🎁 Pull Request: 🛑 Stalled");
    }

    [Fact(DisplayName = "Given Planning State, it should show 'In Progress'")]
    public void ShouldShowInProgress_WhenPlanning()
    {
        // Arrange
        var response = CreateResponse(SessionState.Planning, null);

        // Act
        var output = StatusPresenter.Format(response);

        // Assert
        output.Should().Contain("🎁 Pull Request: ⏳ In Progress");
    }

    [Fact(DisplayName = "Given AwaitingPlanApproval (No PR), it should show 'Awaiting Plan Approval'")]
    public void ShouldShowAwaitingPlanApproval_NoPR()
    {
        // Arrange
        var response = CreateResponse(SessionState.AwaitingPlanApproval, null);

        // Act
        var output = StatusPresenter.Format(response);

        // Assert
        output.Should().Contain("🧘‍♀️ Session State: [Waiting for You]");
        output.Should().Contain("🎁 Pull Request: ⏳ Awaiting Plan Approval");
    }

    [Fact(DisplayName = "Given AwaitingPlanApproval (With PR), it should show 'Awaiting Plan Approval | URL'")]
    public void ShouldShowAwaitingPlanApproval_WithPR()
    {
        // Arrange
        var pr = new PullRequest(new Uri("https://github.com/pr/1"), "PR");
        var response = CreateResponse(SessionState.AwaitingPlanApproval, pr);

        // Act
        var output = StatusPresenter.Format(response);

        // Assert
        output.Should().Contain("🎁 Pull Request: ⏳ Awaiting Plan Approval | https://github.com/pr/1");
    }

    [Fact(DisplayName = "Given Queued State, it should show 'In Progress'")]
    public void ShouldShowInProgress_WhenQueued()
    {
        // Arrange
        var response = CreateResponse(SessionState.Queued, null);

        // Act
        var output = StatusPresenter.Format(response);

        // Assert
        output.Should().Contain("🎁 Pull Request: ⏳ In Progress");
    }

    [Fact(DisplayName = "Given WTF State, it should show 'In Progress'")]
    public void ShouldShowInProgress_WhenWTF()
    {
        // Arrange
        var response = CreateResponse(SessionState.WTF, null);

        // Act
        var output = StatusPresenter.Format(response);

        // Assert
        output.Should().Contain("🎁 Pull Request: ⏳ In Progress");
    }

    [Fact(DisplayName = "Given Last Activity, it should format with timestamp")]
    public void ShouldFormatLastActivity()
    {
        // Arrange
        var timestamp = DateTimeOffset.Parse("2023-10-27T14:30:00Z");
        var activity = new ProgressActivity("a", "r", timestamp, ActivityOriginator.System, "Working on it");
        var response = new RefreshPulseResponse(
            new SessionId("s1"),
            new SessionPulse(SessionStatus.InProgress, "Detail"),
            SessionState.Working,
            DeliveryStatus.Pending,
            activity);

        // Act
        var output = StatusPresenter.Format(response);

        // Assert
        var localTime = timestamp.ToLocalTime().ToString("HH:mm");
        output.Should().Contain($"📝 Last Activity: [{localTime}] Working on it");
    }

    [Fact(DisplayName = "Given ProgressActivity with multi-line Thought, it should be indented")]
    public void ShouldIndentThoughts()
    {
        // Arrange
        var activity = new ProgressActivity("a", "r", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Task", "Line 1\nLine 2");
        var response = new RefreshPulseResponse(
            new SessionId("s1"),
            new SessionPulse(SessionStatus.InProgress, "Detail"),
            SessionState.Working,
            DeliveryStatus.Pending,
            activity);

        // Act
        var output = StatusPresenter.Format(response);

        // Assert
        output.Should().Contain("\n          💭 Line 1");
        output.Should().Contain("\n             Line 2");
    }

    [Fact(DisplayName = "Given SessionAssignedActivity, it should format summary")]
    public void ShouldFormatSessionAssigned()
    {
        // Arrange
        var activity = new SessionAssignedActivity("a", "r", DateTimeOffset.UtcNow, ActivityOriginator.User, (TaskDescription)"Initial Task");
        var response = new RefreshPulseResponse(
            new SessionId("s1"),
            new SessionPulse(SessionStatus.StartingUp, "Start"),
            SessionState.Queued,
            DeliveryStatus.Pending,
            activity);

        // Act
        var output = StatusPresenter.Format(response);

        // Assert
        output.Should().Contain("📝 Last Activity:");
        output.Should().Contain("Session Assigned: Initial Task");
    }

    [Fact(DisplayName = "Given Undefined State, it should fall back to default formatting")]
    public void ShouldHandleUndefinedState()
    {
        // Arrange
        var undefinedState = (SessionState)999;
        var pr = new PullRequest(new Uri("https://github.com/pr/1"), "PR");
        var response = CreateResponse(undefinedState, pr);

        // Act
        var output = StatusPresenter.Format(response);

        // Assert
        output.Should().Contain("🧘‍♀️ Session State: [999]");
        output.Should().Contain("https://github.com/pr/1");
    }

    [Fact(DisplayName = "Given Undefined State (No PR), it should fall back to default progress")]
    public void ShouldHandleUndefinedState_NoPR()
    {
        // Arrange
        var undefinedState = (SessionState)999;
        var response = CreateResponse(undefinedState, null);

        // Act
        var output = StatusPresenter.Format(response);

        // Assert
        output.Should().Contain("🎁 Pull Request: ⏳ In Progress");
    }

    // Helper
    private static RefreshPulseResponse CreateResponse(SessionState state, PullRequest? pr)
    {
        var dummy = new ProgressActivity("a", "r", DateTimeOffset.UtcNow, ActivityOriginator.System, "dummy");
        return new RefreshPulseResponse(
            new SessionId("s1"),
            new SessionPulse(SessionStatus.InProgress, "Detail"),
            state,
            DeliveryStatus.Pending,
            dummy,
            pr);
    }
}
