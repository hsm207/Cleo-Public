using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ViewPlan;
using FluentAssertions;
using Moq;
using Xunit;

namespace Cleo.Core.Tests.UseCases;

public class ViewPlanUseCaseTests
{
    private readonly Mock<ISessionReader> _readerMock;
    private readonly ViewPlanUseCase _useCase;

    public ViewPlanUseCaseTests()
    {
        _readerMock = new Mock<ISessionReader>();
        _useCase = new ViewPlanUseCase(_readerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsyncWhenSessionNotFoundReturnsEmpty()
    {
        // Arrange
        var sessionId = new SessionId("unknown");
        _readerMock.Setup(r => r.RecallAsync(sessionId, TestContext.Current.CancellationToken))
            .ReturnsAsync((Session?)null);

        // Act
        // Use default cancellation token for execute call as it's not the SUT behavior under test here
        var response = await _useCase.ExecuteAsync(new ViewPlanRequest(sessionId), TestContext.Current.CancellationToken);

        // Assert
        response.HasPlan.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsyncWhenSessionHasNoPlanReturnsEmpty()
    {
        // Arrange
        var sessionId = new SessionId("session-no-plan");
        var session = new Session(
            sessionId,
            new TaskDescription("Task"),
            new SourceContext("owner/repo", "main"),
            new SessionPulse(SessionStatus.StartingUp, "Just started")
        );

        _readerMock.Setup(r => r.RecallAsync(sessionId, TestContext.Current.CancellationToken))
            .ReturnsAsync(session);

        // Act
        var response = await _useCase.ExecuteAsync(new ViewPlanRequest(sessionId), TestContext.Current.CancellationToken);

        // Assert
        response.HasPlan.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsyncWhenSessionHasPlanReturnsPlanDetails()
    {
        // Arrange
        var sessionId = new SessionId("session-with-plan");
        var session = new Session(
            sessionId,
            new TaskDescription("Task"),
            new SourceContext("owner/repo", "main"),
            new SessionPulse(SessionStatus.Planning, "Planning")
        );

        var steps = new List<PlanStep> { new(1, "Step 1", "Desc") };
        var planActivity = new PlanningActivity("act-1", DateTimeOffset.UtcNow, "PLAN-A", steps);

        session.AddActivity(planActivity);

        _readerMock.Setup(r => r.RecallAsync(sessionId, TestContext.Current.CancellationToken))
            .ReturnsAsync(session);

        // Act
        var response = await _useCase.ExecuteAsync(new ViewPlanRequest(sessionId), TestContext.Current.CancellationToken);

        // Assert
        response.HasPlan.Should().BeTrue();
        response.PlanId.Should().Be("PLAN-A");
        response.Steps.Should().HaveCount(1);
        response.Steps[0].Title.Should().Be("Step 1");
    }
}
