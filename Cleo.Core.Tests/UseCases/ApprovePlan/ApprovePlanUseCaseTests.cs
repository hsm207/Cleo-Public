using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.ApprovePlan;
using Xunit;

namespace Cleo.Core.Tests.UseCases.ApprovePlan;

public sealed class ApprovePlanUseCaseTests
{
    private readonly FakeMessenger _messenger = new();
    private readonly ApprovePlanUseCase _sut;

    public ApprovePlanUseCaseTests()
    {
        _sut = new ApprovePlanUseCase(_messenger);
    }

    [Fact(DisplayName = "Given a specific Plan ID, when approving the Plan, then a formal 'Approval' Feedback should be transmitted to the Session.")]
    public async Task ShouldTransmitApprovalFeedback()
    {
        // Arrange
        var sessionId = new SessionId("sessions/active-mission");
        var planId = "plan-abc";
        var request = new ApprovePlanRequest(sessionId, planId);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(sessionId, result.Id);
        Assert.Equal(planId, result.PlanId);
        Assert.NotEqual(default, result.ApprovedAt);
        Assert.Contains(planId, _messenger.LastMessage, StringComparison.Ordinal);
        Assert.Contains("approved", _messenger.LastMessage, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeMessenger : ISessionMessenger
    {
        public string LastMessage { get; private set; } = string.Empty;
        public Task SendMessageAsync(SessionId id, string message, CancellationToken cancellationToken = default)
        {
            LastMessage = message;
            return Task.CompletedTask;
        }
    }
}
