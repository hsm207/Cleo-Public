using Cleo.Cli.Presenters;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.RefreshPulse;
using FluentAssertions;
using Xunit;

namespace Cleo.Cli.Tests.Presenters;

public class StatusPresenterTests
{
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
