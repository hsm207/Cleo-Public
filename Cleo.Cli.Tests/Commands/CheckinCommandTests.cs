using Cleo.Cli.Commands;
using Cleo.Cli.Aesthetics;
using Cleo.Cli.Models;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.RefreshPulse;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public class CheckinCommandTests : IDisposable
{
    private readonly Mock<IRefreshPulseUseCase> _useCaseMock;
    private readonly CliStatusPresenter _presenter;
    private readonly Mock<ILogger<CheckinCommand>> _loggerMock;
    private readonly CheckinCommand _command;
    private readonly StringWriter _stringWriter;
    private readonly TextWriter _originalOutput;

    public CheckinCommandTests()
    {
        _useCaseMock = new Mock<IRefreshPulseUseCase>();
        _presenter = new CliStatusPresenter();
        _loggerMock = new Mock<ILogger<CheckinCommand>>();
        _command = new CheckinCommand(_useCaseMock.Object, _presenter, _loggerMock.Object);

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

    [Fact(DisplayName = "Given a valid session ID, when running 'checkin', then it should display SessionState, PR status, and Last Activity.")]
    public async Task Status_WithValidSession_DisplaysDetails()
    {
        // Arrange
        var sessionId = "test-session";
        var pulse = new SessionPulse(SessionStatus.InProgress);
        var state = SessionState.Working;
        var activity = new ProgressActivity("a", "r", DateTimeOffset.UtcNow, ActivityOriginator.System, "dummy");

        _useCaseMock.Setup(x => x.ExecuteAsync(It.Is<RefreshPulseRequest>(r => r.Id.Value == sessionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshPulseResponse(new SessionId(sessionId), pulse, state, activity));

        // Act
        var exitCode = await _command.Build().InvokeAsync($"checkin {sessionId}");

        // Assert
        exitCode.Should().Be(0);
        var output = _stringWriter.ToString();

        output.Should().Contain(CliAesthetic.SessionStateLabel);
        output.Should().Contain("[Working]");
        output.Should().Contain(CliAesthetic.LastActivityLabel);
        output.Should().Contain("dummy");
        output.Should().NotContain("💔 Error");
    }

    [Fact(DisplayName = "Given a session with a PR, when running 'checkin', then it should display the PR URL.")]
    public async Task Status_WithPR_DisplaysPRUrl()
    {
        // Arrange
        var sessionId = "test-session";
        var pulse = new SessionPulse(SessionStatus.AwaitingFeedback);
        var pr = new PullRequest(new Uri("https://github.com/org/repo/pull/1"), "Title", "Open");
        var activity = new ProgressActivity("a", "r", DateTimeOffset.UtcNow, ActivityOriginator.System, "dummy");

        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<RefreshPulseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshPulseResponse(new SessionId(sessionId), pulse, SessionState.AwaitingFeedback, activity, pr));

        // Act
        await _command.Build().InvokeAsync($"checkin {sessionId}");

        // Assert
        _stringWriter.ToString().Should().Contain("https://github.com/org/repo/pull/1");
    }

    [Fact(DisplayName = "Given a response with a warning, when running 'checkin', then it should display the warning.")]
    public async Task Status_WithWarning_DisplaysWarning()
    {
        // Arrange
        var sessionId = "test-session";
        var response = new RefreshPulseResponse(
            new SessionId(sessionId),
            new SessionPulse(SessionStatus.InProgress),
            SessionState.Working,
            new ProgressActivity("a", "r", DateTimeOffset.UtcNow, ActivityOriginator.System, "dummy"),
            Warning: "⚠️ Warning message");

        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<RefreshPulseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _command.Build().InvokeAsync($"checkin {sessionId}");

        // Assert
        _stringWriter.ToString().Should().Contain("⚠️ Warning message");
    }

    [Fact(DisplayName = "Given an error fetching checkin, when running 'checkin', then it should log the error and display a friendly message.")]
    public async Task Status_UseCaseError_HandlesException()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<RefreshPulseRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network unavailable"));

        // Act
        var exitCode = await _command.Build().InvokeAsync("checkin session-123");

        // Assert
        exitCode.Should().Be(0); // Handled exception
        var output = _stringWriter.ToString();
        output.Should().Contain("💔 Error: Network unavailable");

        _loggerMock.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
