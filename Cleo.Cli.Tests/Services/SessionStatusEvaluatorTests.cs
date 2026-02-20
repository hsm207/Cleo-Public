using Cleo.Cli.Models;
using Cleo.Cli.Services;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.RefreshPulse;
using Cleo.Tests.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace Cleo.Cli.Tests.Services;

public sealed class SessionStatusEvaluatorTests
{
    private readonly Mock<IHelpProvider> _helpProviderMock;
    private readonly SessionStatusEvaluator _evaluator;

    public SessionStatusEvaluatorTests()
    {
        _helpProviderMock = new Mock<IHelpProvider>();
        _helpProviderMock.Setup(x => x.GetResource(It.IsAny<string>())).Returns<string>(key => key);
        _evaluator = new SessionStatusEvaluator(_helpProviderMock.Object);
    }

    [Fact(DisplayName = "Given Working State, Evaluator should return correct title and In Progress outcome")]
    public void ShouldEvaluateWorkingState()
    {
        var response = CreateResponse(SessionState.Working, null);
        var vm = _evaluator.Evaluate(response);

        vm.StateTitle.Should().Be("Status_State_Working");
        vm.PrOutcome.Should().Be("Status_PR_InProgress");
    }

    [Fact(DisplayName = "Given Idle State with PR, Evaluator should return Finished and Success outcome")]
    public void ShouldEvaluateIdleWithPr()
    {
        _helpProviderMock.Setup(x => x.GetResource("Status_PR_SuccessWithPR")).Returns("Success: {0}");

        var pr = new PullRequest(new Uri("https://github.com/pr/1"), "PR", "Desc", "feature", "main");
        var response = CreateResponse(SessionState.Idle, pr);
        var vm = _evaluator.Evaluate(response);

        vm.StateTitle.Should().Be("Status_State_Finished");
        vm.PrOutcome.Should().Be("Success: feature | https://github.com/pr/1");
    }

    [Fact(DisplayName = "Given Idle State without PR, Evaluator should return WTF outcome")]
    public void ShouldEvaluateIdleWithoutPr()
    {
        var response = CreateResponse(SessionState.Idle, null);
        var vm = _evaluator.Evaluate(response);

        vm.StateTitle.Should().Be("Status_State_Finished");
        vm.PrOutcome.Should().Be("Status_PR_FinishedNoPR");
    }

    [Fact(DisplayName = "Given Unsubmitted Solution (Ghost Mode), Evaluator should return explicit warning.")]
    public void ShouldEvaluateUnsubmittedSolution()
    {
        // Arrange
        // No PR, but HasUnsubmittedSolution = true
        var response = CreateResponse(SessionState.Idle, null, hasUnsubmittedSolution: true);

        // Act
        var vm = _evaluator.Evaluate(response);

        // Assert
        vm.PrOutcome.Should().Be("Status_PR_SolutionReady");
    }

    [Fact(DisplayName = "Given AwaitingPlanApproval, Evaluator should return Waiting for You")]
    public void ShouldEvaluateAwaitingPlanApproval()
    {
        var response = CreateResponse(SessionState.AwaitingPlanApproval, null);
        var vm = _evaluator.Evaluate(response);

        vm.StateTitle.Should().Be("Status_State_Waiting");
        vm.PrOutcome.Should().Be("Status_PR_AwaitingPlanApproval");
    }

    [Fact(DisplayName = "Given Broken State, Evaluator should return Stalled outcome")]
    public void ShouldEvaluateBrokenState()
    {
        var response = CreateResponse(SessionState.Broken, null);
        var vm = _evaluator.Evaluate(response);

        vm.StateTitle.Should().Be("Status_State_Stalled");
        vm.PrOutcome.Should().Be("Status_PR_Stalled");
    }

    [Fact(DisplayName = "Given Interrupted State with PR, Evaluator should return Stalled outcome with URL")]
    public void ShouldEvaluateInterruptedStateWithPR()
    {
        _helpProviderMock.Setup(x => x.GetResource("Status_PR_StalledWithPR")).Returns("Stalled: {0}");

        var pr = new PullRequest(new Uri("https://github.com/pr/1"), "PR", "desc", "feature", "main");
        var response = CreateResponse(SessionState.Interrupted, pr);
        var vm = _evaluator.Evaluate(response);

        vm.StateTitle.Should().Be("Status_State_Stalled");
        vm.PrOutcome.Should().Be("Stalled: feature | https://github.com/pr/1");
    }

    [Fact(DisplayName = "Given AwaitingFeedback with PR, Evaluator should return Awaiting response outcome")]
    public void ShouldEvaluateAwaitingFeedbackWithPR()
    {
        _helpProviderMock.Setup(x => x.GetResource("Status_PR_AwaitingResponseWithPR")).Returns("Waiting: {0}");

        var pr = new PullRequest(new Uri("https://github.com/pr/1"), "PR", "desc", "feature", "main");
        var response = CreateResponse(SessionState.AwaitingFeedback, pr);
        var vm = _evaluator.Evaluate(response);

        vm.PrOutcome.Should().Be("Waiting: feature | https://github.com/pr/1");
    }

    [Fact(DisplayName = "Given Planning with PR, Evaluator should return Iterating outcome")]
    public void ShouldEvaluatePlanningWithPR()
    {
        _helpProviderMock.Setup(x => x.GetResource("Status_PR_Iterating")).Returns("Iterating: {0}");

        var pr = new PullRequest(new Uri("https://github.com/pr/1"), "PR", "desc", "feature", "main");
        var response = CreateResponse(SessionState.Planning, pr);
        var vm = _evaluator.Evaluate(response);

        vm.PrOutcome.Should().Be("Iterating: feature | https://github.com/pr/1");
    }

    [Fact(DisplayName = "Given AwaitingPlanApproval with PR, Evaluator should return Awaiting Approval outcome")]
    public void ShouldEvaluateAwaitingPlanApprovalWithPR()
    {
        _helpProviderMock.Setup(x => x.GetResource("Status_PR_AwaitingPlanApprovalWithPR")).Returns("Approval: {0}");

        var pr = new PullRequest(new Uri("https://github.com/pr/1"), "PR", "desc", "feature", "main");
        var response = CreateResponse(SessionState.AwaitingPlanApproval, pr);
        var vm = _evaluator.Evaluate(response);

        vm.PrOutcome.Should().Be("Approval: feature | https://github.com/pr/1");
    }

    [Fact(DisplayName = "Given Unknown State, FormatStateTitle should return default string.")]
    public void ShouldEvaluateUnknownState()
    {
        // 999 is not a valid enum member usually, but we cast it to force default case
        var response = CreateResponse((SessionState)999, null);
        var vm = _evaluator.Evaluate(response);

        vm.StateTitle.Should().Be("999");
    }

    [Fact(DisplayName = "Given Unknown State without PR, EvaluatePrOutcome should return InProgress.")]
    public void ShouldEvaluateUnknownState_NoPR()
    {
        _helpProviderMock.Setup(x => x.GetResource("Status_PR_InProgress")).Returns("DefaultProgress");

        var response = CreateResponse((SessionState)999, null);
        var vm = _evaluator.Evaluate(response);

        vm.PrOutcome.Should().Be("DefaultProgress");
    }

    [Fact(DisplayName = "Given Unknown State with PR, EvaluatePrOutcome should return raw PR info.")]
    public void ShouldEvaluateUnknownState_WithPR()
    {
        var pr = new PullRequest(new Uri("https://github.com/pr/1"), "PR", "desc", "feature", "main");
        var response = CreateResponse((SessionState)999, pr);
        var vm = _evaluator.Evaluate(response);

        // Expects default format "{0}" which just outputs the PR info
        vm.PrOutcome.Should().Be("feature | https://github.com/pr/1");
    }

    [Fact(DisplayName = "Given AwaitingFeedback without PR, Evaluator should return Awaiting Response outcome")]
    public void ShouldEvaluateAwaitingFeedbackWithoutPR()
    {
        _helpProviderMock.Setup(x => x.GetResource("Status_PR_AwaitingResponse")).Returns("WaitingNoPR");

        var response = CreateResponse(SessionState.AwaitingFeedback, null);
        var vm = _evaluator.Evaluate(response);

        vm.PrOutcome.Should().Be("WaitingNoPR");
    }

    private static RefreshPulseResponse CreateResponse(SessionState state, PullRequest? pr, bool hasUnsubmittedSolution = false)
    {
        var dummy = new ProgressActivity("a", "r", DateTimeOffset.UtcNow, ActivityOriginator.System, "dummy");
        return new RefreshPulseResponse(
            TestFactory.CreateSessionId("s1"),
            new SessionPulse(SessionStatus.InProgress),
            state,
            dummy,
            pr,
            HasUnsubmittedSolution: hasUnsubmittedSolution);
    }
}
