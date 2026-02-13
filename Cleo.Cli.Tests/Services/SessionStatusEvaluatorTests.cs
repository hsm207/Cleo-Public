using Cleo.Cli.Models;
using Cleo.Cli.Services;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.RefreshPulse;
using FluentAssertions;
using Xunit;

namespace Cleo.Cli.Tests.Services;

public class SessionStatusEvaluatorTests
{
    private readonly SessionStatusEvaluator _sut = new();

    [Fact(DisplayName = "Given Working State, Evaluator should return correct title and In Progress outcome")]
    public void ShouldEvaluateWorkingState()
    {
        var response = CreateResponse(SessionState.Working, null);
        var vm = _sut.Evaluate(response);

        vm.StateTitle.Should().Be("Working");
        vm.PrOutcome.Should().Be("⏳ In Progress");
    }

    [Fact(DisplayName = "Given Idle State with PR, Evaluator should return Finished and Success outcome")]
    public void ShouldEvaluateIdleWithPr()
    {
        var pr = new PullRequest(new Uri("https://github.com/pr/1"), "PR");
        var response = CreateResponse(SessionState.Idle, pr);
        var vm = _sut.Evaluate(response);

        vm.StateTitle.Should().Be("Finished");
        vm.PrOutcome.Should().Be("✅ https://github.com/pr/1");
    }

    [Fact(DisplayName = "Given AwaitingPlanApproval, Evaluator should return Waiting for You")]
    public void ShouldEvaluateAwaitingPlanApproval()
    {
        var response = CreateResponse(SessionState.AwaitingPlanApproval, null);
        var vm = _sut.Evaluate(response);

        vm.StateTitle.Should().Be("Waiting for You");
        vm.PrOutcome.Should().Be("⏳ Awaiting Plan Approval");
    }

    private static RefreshPulseResponse CreateResponse(SessionState state, PullRequest? pr)
    {
        var dummy = new ProgressActivity("a", "r", DateTimeOffset.UtcNow, ActivityOriginator.System, "dummy");
        return new RefreshPulseResponse(
            new SessionId("s1"),
            new SessionPulse(SessionStatus.InProgress),
            state,
            DeliveryStatus.Pending,
            dummy,
            pr);
    }
}
