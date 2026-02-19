using Cleo.Cli.Commands;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ApprovePlan;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public sealed class ApproveCommandTests
{
    private readonly Mock<IApprovePlanUseCase> _useCaseMock;
    private readonly Mock<IStatusPresenter> _presenterMock;
    private readonly Mock<IHelpProvider> _helpProviderMock;
    private readonly ApproveCommand _command;

    public ApproveCommandTests()
    {
        _useCaseMock = new Mock<IApprovePlanUseCase>();
        _presenterMock = new Mock<IStatusPresenter>();
        _helpProviderMock = new Mock<IHelpProvider>();

        _helpProviderMock.Setup(x => x.GetCommandDescription(It.IsAny<string>())).Returns<string>(k => k);
        _helpProviderMock.Setup(x => x.GetResource(It.IsAny<string>())).Returns<string>(k => "{0} {1} {2}");

        _command = new ApproveCommand(
            _useCaseMock.Object,
            _presenterMock.Object,
            _helpProviderMock.Object,
            new Mock<ILogger<ApproveCommand>>().Object);
    }

    [Fact(DisplayName = "Approve should call UseCase and PresentSuccess.")]
    public async Task Approve_Valid_ApprovesPlan()
    {
        // Arrange
        var planId = new PlanId("plans/123");
        var sessionId = new SessionId("sessions/abc");

        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ApprovePlanRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApprovePlanResponse(sessionId, planId, DateTimeOffset.UtcNow));

        // Act
        await _command.Build().InvokeAsync($"approve {sessionId} {planId}");

        // Assert
        _useCaseMock.Verify(x => x.ExecuteAsync(It.Is<ApprovePlanRequest>(r => r.PlanId == planId && r.Id == sessionId), It.IsAny<CancellationToken>()), Times.Once);
        _presenterMock.Verify(x => x.PresentSuccess(It.IsAny<string>()), Times.Once);
    }

    [Fact(DisplayName = "Approve should call PresentError on exception.")]
    public async Task Approve_Error_PresentsError()
    {
        // Arrange
        var planId = new PlanId("plans/123");
        var sessionId = new SessionId("sessions/abc");

        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ApprovePlanRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Fail"));

        // Act
        await _command.Build().InvokeAsync($"approve {sessionId} {planId}");

        // Assert
        _presenterMock.Verify(x => x.PresentError("Fail"), Times.Once);
    }
}
