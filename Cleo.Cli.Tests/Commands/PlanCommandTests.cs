using Cleo.Cli.Commands;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ApprovePlan;
using Cleo.Core.UseCases.ViewPlan;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public class PlanCommandTests : IDisposable
{
    private readonly Mock<IViewPlanUseCase> _viewPlanUseCaseMock;
    private readonly Mock<ILogger<PlanCommand>> _loggerMock;
    private readonly PlanCommand _command;
    private readonly StringWriter _stringWriter;
    private readonly TextWriter _originalOutput;

    public PlanCommandTests()
    {
        // Setup dependency chain for PlanCommand
        var approveUseCase = new Mock<IApprovePlanUseCase>();
        var approveLogger = new Mock<ILogger<ApproveCommand>>();
        var approveCommand = new ApproveCommand(approveUseCase.Object, approveLogger.Object);

        _viewPlanUseCaseMock = new Mock<IViewPlanUseCase>();
        _loggerMock = new Mock<ILogger<PlanCommand>>();
        _command = new PlanCommand(approveCommand, _viewPlanUseCaseMock.Object, _loggerMock.Object);

        _stringWriter = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_stringWriter);
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _stringWriter.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Given a session with a plan, when running 'plan view', then it should display the plan details.")]
    public async Task View_WithPlan_DisplaysDetails()
    {
        // Arrange
        var sessionId = "sessions/test-session";
        var steps = new List<PlanStepModel>
        {
            new(1, "Do thing", "Desc") // PlanStepModel(int Index, string Title, string Description)
        };
        // ViewPlanResponse(bool HasPlan, string? PlanId, DateTimeOffset? Timestamp, IReadOnlyList<PlanStepModel> Steps, bool IsApproved)
        var response = new ViewPlanResponse(true, new PlanId("plans/plan-123"), DateTimeOffset.UtcNow, steps, true);

        _viewPlanUseCaseMock.Setup(x => x.ExecuteAsync(It.Is<ViewPlanRequest>(r => r.SessionId.Value == sessionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var exitCode = await _command.Build().InvokeAsync($"plan view {sessionId}");

        // Assert
        exitCode.Should().Be(0);
        var output = _stringWriter.ToString();

        output.Should().Contain("🗺️ Approved Plan: plans/plan-123");
        output.Should().Contain("1. Do thing");
    }

    [Fact(DisplayName = "Given a proposed plan (not approved), when running 'plan view', then it should display 'Proposed Plan'.")]
    public async Task View_ProposedPlan_DisplaysProposedTitle()
    {
        // Arrange
        var sessionId = "sessions/test-session";
        var steps = new List<PlanStepModel> { new(1, "Step 1", "Desc") };
        var response = new ViewPlanResponse(true, new PlanId("plans/plan-123"), DateTimeOffset.UtcNow, steps, false); // IsApproved = false

        _viewPlanUseCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ViewPlanRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var exitCode = await _command.Build().InvokeAsync($"plan view {sessionId}");

        // Assert
        exitCode.Should().Be(0);
        var output = _stringWriter.ToString();

        output.Should().Contain("🗺️ Proposed Plan: plans/plan-123");
    }

    [Fact(DisplayName = "Given a plan with descriptions, when running 'plan view', then it should display the descriptions with indentation.")]
    public async Task View_WithDescription_DisplaysIndentedDescription()
    {
        // Arrange
        var sessionId = "sessions/test-session";
        var description = "First line\nSecond line";
        var steps = new List<PlanStepModel>
        {
            new(1, "Step Title", description)
        };
        var response = new ViewPlanResponse(true, new PlanId("plans/plan-123"), DateTimeOffset.UtcNow, steps, true);

        _viewPlanUseCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ViewPlanRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var exitCode = await _command.Build().InvokeAsync($"plan view {sessionId}");

        // Assert
        exitCode.Should().Be(0);
        var output = _stringWriter.ToString();

        output.Should().Contain("   First line");
        output.Should().Contain("   Second line");
    }

    [Fact(DisplayName = "Given a session with no plan, when running 'plan view', then it should display a friendly message.")]
    public async Task View_NoPlan_DisplaysMessage()
    {
        // Arrange
        var sessionId = "sessions/test-session";
        var response = ViewPlanResponse.Empty(); // HasPlan = false

        _viewPlanUseCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ViewPlanRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _command.Build().InvokeAsync($"plan view {sessionId}");

        // Assert
        _stringWriter.ToString().Should().Contain("📭 No approved plan found");
    }

    [Fact(DisplayName = "Given an error, when running 'plan view', then it should log and display error.")]
    public async Task View_Error_HandlesException()
    {
        // Arrange
        _viewPlanUseCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ViewPlanRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var exitCode = await _command.Build().InvokeAsync("plan view sessions/s1");

        // Assert
        exitCode.Should().Be(0);
        _stringWriter.ToString().Should().Contain("💔 Error: Database error");

        _loggerMock.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
    }
}
