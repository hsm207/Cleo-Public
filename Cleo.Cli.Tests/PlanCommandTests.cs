using System.CommandLine;
using System.CommandLine.IO;
using Cleo.Cli.Commands;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ApprovePlan;
using Cleo.Core.UseCases.ViewPlan;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Cleo.Cli.Tests;

// NOTE: TestParallelization is disabled in assembly info if using Console SetOut,
// but here we are using IConsole injection which System.CommandLine supports.
// However, the commands are writing to Console.WriteLine directly instead of IConsole.
// We need to check if the Commands are using IConsole or Console.WriteLine.
// Checking PlanCommand.cs... it uses Console.WriteLine!
// That's why the output is empty in TestConsole.

[Collection("ConsoleTests")]
public class PlanCommandTests : IDisposable
{
    private readonly Mock<IApprovePlanUseCase> _approveUseCaseMock;
    private readonly Mock<ILogger<ApproveCommand>> _approveLoggerMock;
    private readonly Mock<IViewPlanUseCase> _viewUseCaseMock;
    private readonly Mock<ILogger<PlanCommand>> _loggerMock;
    private readonly PlanCommand _command;
    private readonly StringWriter _stringWriter;
    private readonly TextWriter _originalOutput;

    public PlanCommandTests()
    {
        _approveUseCaseMock = new Mock<IApprovePlanUseCase>();
        _approveLoggerMock = new Mock<ILogger<ApproveCommand>>();

        var approveCommand = new ApproveCommand(_approveUseCaseMock.Object, _approveLoggerMock.Object);

        _viewUseCaseMock = new Mock<IViewPlanUseCase>();
        _loggerMock = new Mock<ILogger<PlanCommand>>();

        _command = new PlanCommand(approveCommand, _viewUseCaseMock.Object, _loggerMock.Object);

        _stringWriter = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_stringWriter);
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _stringWriter.Dispose();
    }

    [Fact(DisplayName = "Given an active session with a plan, when running 'plan view', then it should display the authoritative roadmap.")]
    public async Task ViewPlan_WithActivePlan_DisplaysRoadmap()
    {
        // Arrange
        var sessionId = "test-session";
        var planId = "PLAN-123";
        var timestamp = DateTimeOffset.UtcNow;
        var steps = new List<PlanStepDto>
        {
            new(1, "Step One", "Desc 1"),
            new(2, "Step Two", "Desc 2")
        };

        _viewUseCaseMock.Setup(x => x.ExecuteAsync(It.Is<ViewPlanRequest>(r => r.SessionId.Value == sessionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ViewPlanResponse(true, planId, timestamp, steps));

        var command = _command.Build();

        // Act
        var exitCode = await command.InvokeAsync($"view {sessionId}");

        // Assert
        exitCode.Should().Be(0);
        var output = _stringWriter.ToString();

        output.Should().Contain($"🗺️ Authoritative Plan: {planId}");
        output.Should().Contain("1. Step One");
        output.Should().Contain("2. Step Two");
    }

    [Fact(DisplayName = "Given a session with no plan, when running 'plan view', then it should display the empty message.")]
    public async Task ViewPlan_WithNoPlan_DisplaysMessage()
    {
        // Arrange
        var sessionId = "test-session";
        _viewUseCaseMock.Setup(x => x.ExecuteAsync(It.Is<ViewPlanRequest>(r => r.SessionId.Value == sessionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ViewPlanResponse.Empty());

        var command = _command.Build();

        // Act
        var exitCode = await command.InvokeAsync($"view {sessionId}");

        // Assert
        exitCode.Should().Be(0);
        var output = _stringWriter.ToString();

        output.Should().Contain("📭 No authoritative plan found for this session.");
    }
}
