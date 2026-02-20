using Cleo.Cli.Commands;
using Cleo.Cli.Models;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.RefreshPulse;
using Cleo.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public sealed class CheckinCommandTests : IDisposable
{
    private readonly Mock<IRefreshPulseUseCase> _useCaseMock;
    private readonly Mock<IStatusPresenter> _presenterMock;
    private readonly Mock<IHelpProvider> _helpProviderMock;
    private readonly Mock<ISessionStatusEvaluator> _evaluatorMock;
    private readonly Mock<ILogger<CheckinCommand>> _loggerMock;
    private readonly CheckinCommand _command;

    public CheckinCommandTests()
    {
        _useCaseMock = new Mock<IRefreshPulseUseCase>();
        _presenterMock = new Mock<IStatusPresenter>();
        _helpProviderMock = new Mock<IHelpProvider>();
        _evaluatorMock = new Mock<ISessionStatusEvaluator>();
        _loggerMock = new Mock<ILogger<CheckinCommand>>();

        _helpProviderMock.Setup(x => x.GetResource("Cmd_Checkin_Name")).Returns("checkin");
        _helpProviderMock.Setup(x => x.GetResource("Arg_SessionId_Name")).Returns("sessionId");
        _helpProviderMock.Setup(x => x.GetCommandDescription(It.IsAny<string>())).Returns<string>(k => k);

        _command = new CheckinCommand(
            _useCaseMock.Object,
            _presenterMock.Object,
            _helpProviderMock.Object,
            _evaluatorMock.Object,
            _loggerMock.Object);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Given a valid session ID, when running 'checkin', then it should display SessionState.")]
    public async Task Status_WithValidSession_DisplaysDetails()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("sessions/test-session");
        var pulse = new SessionPulse(SessionStatus.InProgress);
        var state = SessionState.Working;

        var activity = new ProgressActivity(
            "act-1",
            "remote-1",
            DateTimeOffset.UtcNow,
            ActivityOriginator.System,
            Intent: "Working hard",
            Reasoning: "Doing things",
            ExecutiveSummary: null);

        var response = new RefreshPulseResponse(
            sessionId,
            pulse,
            state,
            activity,
            PullRequest: null,
            Warning: null,
            HasUnsubmittedSolution: false);

        _useCaseMock.Setup(x => x.ExecuteAsync(It.Is<RefreshPulseRequest>(r => r.Id.Value == sessionId.Value), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var expectedVm = new StatusViewModel("Working", "None", "10:00", "Working hard", null, new List<string>().AsReadOnly(), new List<string>().AsReadOnly());
        _evaluatorMock.Setup(x => x.Evaluate(response)).Returns(expectedVm);

        // Act
        var exitCode = await _command.Build().InvokeAsync($"checkin {sessionId}");

        // Assert
        exitCode.Should().Be(0);

        _presenterMock.Verify(x => x.PresentStatus(expectedVm), Times.Once);
    }

    [Fact(DisplayName = "Given a response with a warning, when running 'checkin', then it should display the warning.")]
    public async Task Status_WithWarning_DisplaysWarning()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("sessions/test-session");
        var response = new RefreshPulseResponse(
            sessionId,
            new SessionPulse(SessionStatus.InProgress),
            SessionState.Working,
            new ProgressActivity("act-1", "rem-1", DateTimeOffset.UtcNow, ActivityOriginator.System, "dummy"),
            PullRequest: null,
            Warning: "⚠️ Warning message",
            HasUnsubmittedSolution: false);

        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<RefreshPulseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _command.Build().InvokeAsync($"checkin {sessionId}");

        // Assert
        _presenterMock.Verify(x => x.PresentWarning("⚠️ Warning message"), Times.Once);
        _presenterMock.Verify(x => x.PresentStatus(It.IsAny<StatusViewModel>()), Times.Once); // Still calls status present
    }

    [Fact(DisplayName = "Given an error fetching checkin, when running 'checkin', then it should log the error and display a friendly message.")]
    public async Task Status_UseCaseError_HandlesException()
    {
        // Arrange
        var exception = new Exception("Network unavailable");
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<RefreshPulseRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var exitCode = await _command.Build().InvokeAsync("checkin sessions/session-123");

        // Assert
        exitCode.Should().Be(0); // Handled exception

        _presenterMock.Verify(x => x.PresentError("Network unavailable"), Times.Once);

        _loggerMock.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
