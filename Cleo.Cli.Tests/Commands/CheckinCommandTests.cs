using Cleo.Cli.Commands;
using Cleo.Cli.Aesthetics;
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
using System.CommandLine.IO;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public class CheckinCommandTests : IDisposable
{
    private readonly Mock<IRefreshPulseUseCase> _useCaseMock;
    private readonly CliStatusPresenter _presenter;
    private readonly Mock<IHelpProvider> _helpProviderMock;
    private readonly Mock<ILogger<CheckinCommand>> _loggerMock;
    private readonly CheckinCommand _command;
    private readonly TestConsole _testConsole;

    public CheckinCommandTests()
    {
        _useCaseMock = new Mock<IRefreshPulseUseCase>();
        _helpProviderMock = new Mock<IHelpProvider>();
        _testConsole = new TestConsole();
        _presenter = new CliStatusPresenter(_testConsole, _helpProviderMock.Object);
        _loggerMock = new Mock<ILogger<CheckinCommand>>();

        // Setup HelpProvider to return basic format strings for errors
        _helpProviderMock.Setup(x => x.GetResource("New_Error")).Returns("💔 Error: {0}");

        _command = new CheckinCommand(_useCaseMock.Object, _presenter, _loggerMock.Object);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Given a valid session ID, when running 'checkin', then it should display SessionState, PR status, and Last Activity.")]
    public async Task Status_WithValidSession_DisplaysDetails()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("sessions/test-session");
        var pulse = new SessionPulse(SessionStatus.InProgress);
        var state = SessionState.Working;

        // ProgressActivity(Id, RemoteId, Timestamp, Originator, Intent, Reasoning, Evidence, ExecutiveSummary)
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

        // Act
        var exitCode = await _command.Build().InvokeAsync($"checkin {sessionId}");

        // Assert
        exitCode.Should().Be(0);
        var output = _testConsole.Out.ToString()!;

        output.Should().Contain(CliAesthetic.SessionStateLabel);
        output.Should().Contain("[Working]");
        output.Should().Contain(CliAesthetic.LastActivityLabel);
        output.Should().Contain("Working hard");
    }

    [Fact(DisplayName = "Given a session with a PR, when running 'checkin', then it should display the PR URL.")]
    public async Task Status_WithPR_DisplaysPRUrl()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("sessions/test-session");
        var pulse = new SessionPulse(SessionStatus.AwaitingFeedback);
        var pr = new PullRequest(new Uri("https://github.com/org/repo/pull/1"), "Title", "Open", "head", "base");
        var activity = new ProgressActivity("act-1", "rem-1", DateTimeOffset.UtcNow, ActivityOriginator.System, "dummy");

        var response = new RefreshPulseResponse(
            sessionId,
            pulse,
            SessionState.AwaitingFeedback,
            activity,
            PullRequest: pr,
            Warning: null,
            HasUnsubmittedSolution: false);

        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<RefreshPulseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _command.Build().InvokeAsync($"checkin {sessionId}");

        // Assert
        _testConsole.Out.ToString().Should().Contain("https://github.com/org/repo/pull/1");
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
        _testConsole.Out.ToString().Should().Contain("⚠️ Warning message");
    }

    [Fact(DisplayName = "Given an error fetching checkin, when running 'checkin', then it should log the error and display a friendly message.")]
    public async Task Status_UseCaseError_HandlesException()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<RefreshPulseRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network unavailable"));

        // Act
        var exitCode = await _command.Build().InvokeAsync("checkin sessions/session-123");

        // Assert
        exitCode.Should().Be(0); // Handled exception
        var output = _testConsole.Out.ToString()!;
        output.Should().Contain("💔 Error: Network unavailable");

        _loggerMock.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
