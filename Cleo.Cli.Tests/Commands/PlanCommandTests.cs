using Cleo.Cli.Commands;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ViewPlan;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public sealed class PlanCommandTests
{
    private readonly Mock<IViewPlanUseCase> _useCaseMock;
    private readonly Mock<IStatusPresenter> _presenterMock;
    private readonly Mock<IHelpProvider> _helpProviderMock;
    private readonly PlanCommand _command;

    public PlanCommandTests()
    {
        _useCaseMock = new Mock<IViewPlanUseCase>();
        _presenterMock = new Mock<IStatusPresenter>();
        _helpProviderMock = new Mock<IHelpProvider>();

        // Fix mocks
        _helpProviderMock.Setup(x => x.GetResource(It.IsAny<string>())).Returns<string>(key =>
            key switch {
                "Cmd_Plan_Name" => "plan",
                "Cmd_View_Name" => "view",
                "Arg_SessionId_Name" => "sessionId",
                "Cmd_Approve_Name" => "approve", // Recursively used
                "Arg_PlanId_Name" => "planId",
                _ => key
            });
        _helpProviderMock.Setup(x => x.GetCommandDescription(It.IsAny<string>())).Returns<string>(k => k);

        var approveCommand = new ApproveCommand(
            new Mock<Core.UseCases.ApprovePlan.IApprovePlanUseCase>().Object,
            _presenterMock.Object,
            _helpProviderMock.Object,
            new Mock<ILogger<ApproveCommand>>().Object);

        _command = new PlanCommand(
            approveCommand,
            _useCaseMock.Object,
            _presenterMock.Object,
            _helpProviderMock.Object,
            new Mock<ILogger<PlanCommand>>().Object);
    }

    [Fact(DisplayName = "View plan should call UseCase and PresentPlan.")]
    public async Task View_Valid_PresentsPlan()
    {
        // Arrange
        var sessionId = new SessionId("sessions/123");
        var response = new ViewPlanResponse(true, new PlanId("plans/456"), DateTimeOffset.UtcNow, new[] { new PlanStepModel(1, "Step 1", "Desc") }, true);

        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ViewPlanRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _command.Build().InvokeAsync($"plan view {sessionId}");

        // Assert
        _presenterMock.Verify(x => x.PresentPlan(response), Times.Once);
    }

    [Fact(DisplayName = "View plan should call PresentEmptyPlan when no plan.")]
    public async Task View_Empty_PresentsEmpty()
    {
        // Arrange
        var sessionId = new SessionId("sessions/123");
        var response = ViewPlanResponse.Empty();

        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ViewPlanRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _command.Build().InvokeAsync($"plan view {sessionId}");

        // Assert
        _presenterMock.Verify(x => x.PresentEmptyPlan(), Times.Once);
    }

    [Fact(DisplayName = "View plan should call PresentError on exception.")]
    public async Task View_Error_PresentsError()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ViewPlanRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Fail"));

        // Act
        await _command.Build().InvokeAsync($"plan view sessions/123");

        // Assert
        _presenterMock.Verify(x => x.PresentError("Fail"), Times.Once);
    }
}
