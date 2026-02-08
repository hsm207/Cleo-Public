using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.ApprovePlan;
using Xunit;

namespace Cleo.Core.Tests.UseCases.ApprovePlan;

public sealed class ApprovePlanUseCaseTests
{
    private readonly FakeController _controller = new();
    private readonly ApprovePlanUseCase _sut;

    public ApprovePlanUseCaseTests()
    {
        _sut = new ApprovePlanUseCase(_controller);
    }

    [Fact(DisplayName = "Given a specific Plan ID, when approving the Plan, then a formal 'Approval' signal should be transmitted via the Controller Port.")]
    public async Task ShouldTransmitApprovalSignal()
    {
        // Arrange
        var sessionId = new SessionId("sessions/active-session");
        var planId = "plan-abc";
        var request = new ApprovePlanRequest(sessionId, planId);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(sessionId, result.Id);
        Assert.Equal(planId, result.PlanId);
        Assert.NotEqual(default, result.ApprovedAt);
        Assert.True(_controller.WasApproved, "The approval signal was not sent to the controller.");
    }

    private sealed class FakeController : ISessionController
    {
        public bool WasApproved { get; private set; }
        
        public Task ApprovePlanAsync(SessionId id, CancellationToken cancellationToken = default)
        {
            WasApproved = true;
            return Task.CompletedTask;
        }
    }
}
