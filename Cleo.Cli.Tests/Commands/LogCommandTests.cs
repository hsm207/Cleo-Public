using Cleo.Cli.Commands;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.BrowseHistory;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

// Disable parallelization to avoid Console output capture issues
[Collection("ConsoleTests")]
public class LogCommandTests : IDisposable
{
    private readonly Mock<IBrowseHistoryUseCase> _useCaseMock;
    private readonly Mock<ILogger<LogCommand>> _loggerMock;
    private readonly LogCommand _command;
    private readonly StringWriter _stringWriter;
    private readonly TextWriter _originalOutput;

    public LogCommandTests()
    {
        _useCaseMock = new Mock<IBrowseHistoryUseCase>();
        _loggerMock = new Mock<ILogger<LogCommand>>();
        _command = new LogCommand(_useCaseMock.Object, _loggerMock.Object);

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

    [Fact(DisplayName = "Given a valid session ID, when running 'log view', then it should display the history.")]
    public async Task View_WithActivities_DisplaysHistory()
    {
        // Arrange
        var sessionId = new SessionId("test-session");
        var timestamp = DateTimeOffset.UtcNow;
        var history = new List<SessionActivity>
        {
            new MessageActivity("msg-1", timestamp, ActivityOriginator.User, "Hello Jules!")
        };

        SetupUseCase(sessionId.Value, history);

        // Act
        var exitCode = await _command.Build().InvokeAsync($"log view {sessionId.Value}");

        // Assert
        exitCode.Should().Be(0);
        var output = _stringWriter.ToString();

        output.Should().Contain($"Activities for {sessionId.Value}");
        output.Should().Contain("Hello Jules!");
    }

    [Fact(DisplayName = "Given a noisy session log, when viewing the narrative summary, then it should exclude technical heartbeats and show gap markers.")]
    public async Task GivenNoisyLog_WhenViewingSummary_ThenExcludesHeartbeatsAndShowsGapMarkers()
    {
        // Arrange
        var sessionId = "test-session";
        var now = DateTimeOffset.UtcNow;
        var history = new List<SessionActivity>
        {
            new MessageActivity("msg-1", now, ActivityOriginator.User, "Start"), // Sig
            new ProgressActivity("prog-1", now, ActivityOriginator.Agent, "working"), // Non-Sig (Gap)
            new ProgressActivity("prog-2", now, ActivityOriginator.Agent, "working more"), // Non-Sig
            new MessageActivity("msg-2", now, ActivityOriginator.Agent, "Done") // Sig
        };
        // Total Sig: 2. Limit: 10. Displayed: 2. Truncated: 0. Hidden Heartbeats: 2. Gap between msg-1 and msg-2 is 2.

        SetupUseCase(sessionId, history);

        // Act
        await _command.Build().InvokeAsync($"log view {sessionId}");

        // Assert
        var output = _stringWriter.ToString();
        output.Should().Contain("Start");
        output.Should().Contain("Done");
        output.Should().NotContain("working");
        output.Should().Contain("... [2 heartbeats hidden] ...");
        output.Should().Contain("Showing 2 of 2 significant activities (2 total heartbeats hidden).");
    }

    [Fact(DisplayName = "Given a session log with many milestones, when viewing the summary, then it should show truncation markers and the detailed informational footer.")]
    public async Task GivenManyMilestones_WhenViewingSummary_ThenShowsTruncationAndFooter()
    {
        // Arrange
        var sessionId = "test-session";
        var now = DateTimeOffset.UtcNow;
        var history = new List<SessionActivity>();

        // 5 truncated sigs
        for (int i = 0; i < 5; i++) history.Add(new MessageActivity($"old-{i}", now, ActivityOriginator.User, $"Old {i}"));

        // 50 heartbeats
        for (int i = 0; i < 50; i++) history.Add(new ProgressActivity($"noise-{i}", now, ActivityOriginator.Agent, $"Noise {i}"));

        // 10 displayed sigs
        for (int i = 0; i < 10; i++) history.Add(new MessageActivity($"new-{i}", now, ActivityOriginator.User, $"New {i}"));

        SetupUseCase(sessionId, history);

        // Act
        await _command.Build().InvokeAsync($"log view {sessionId}");

        // Assert
        var output = _stringWriter.ToString();
        output.Should().Contain("... [5 earlier activities hidden] ..."); // Truncation marker
        output.Should().Contain("New 0");
        output.Should().Contain("New 9");
        output.Should().NotContain("Old 4");

        // Gap check: Between Old-4 (index 4) and New-0 (index 55).
        // 50 heartbeats hidden.
        output.Should().Contain("... [50 heartbeats hidden] ...");

        output.Should().Contain("Showing 10 of 15 significant activities (50 total heartbeats hidden).");
    }

    [Fact(DisplayName = "Given a session log with many milestones, when viewing with a custom limit, then it should display the requested number of activities and update the markers and footer.")]
    public async Task GivenCustomLimit_WhenViewing_ThenRespectsLimit()
    {
        // Arrange
        var sessionId = "test-session";
        var now = DateTimeOffset.UtcNow;
        var history = new List<SessionActivity>();

        // 10 truncated sigs
        for(int i=0; i<10; i++) history.Add(new MessageActivity($"old-{i}", now, ActivityOriginator.User, $"Old {i}"));

        // 20 heartbeats
        for(int i=0; i<20; i++) history.Add(new ProgressActivity($"noise-{i}", now, ActivityOriginator.Agent, $"Noise {i}"));

        // 5 displayed sigs
        for(int i=0; i<5; i++) history.Add(new MessageActivity($"new-{i}", now, ActivityOriginator.User, $"New {i}"));

        SetupUseCase(sessionId, history);

        // Act
        await _command.Build().InvokeAsync($"log view {sessionId} --limit 5");

        // Assert
        var output = _stringWriter.ToString();
        output.Should().Contain("... [10 earlier activities hidden] ...");
        output.Should().Contain("New 0");
        output.Should().NotContain("Old 9");
        output.Should().Contain("... [20 heartbeats hidden] ...");
        output.Should().Contain("Showing 5 of 15 significant activities (20 total heartbeats hidden).");
    }

    [Fact(DisplayName = "Given a noisy session log, when viewing with the --all flag, then it should display every recorded activity without markers.")]
    public async Task GivenNoisyLog_WhenViewingAll_ThenShowsEverything()
    {
        // Arrange
        var sessionId = "test-session";
        var now = DateTimeOffset.UtcNow;
        var history = new List<SessionActivity>
        {
            new MessageActivity("msg-1", now, ActivityOriginator.User, "Sig 1"),
            new ProgressActivity("prog-1", now, ActivityOriginator.Agent, "Noise 1"),
            new MessageActivity("msg-2", now, ActivityOriginator.User, "Sig 2")
        };

        SetupUseCase(sessionId, history);

        // Act
        await _command.Build().InvokeAsync($"log view {sessionId} --all");

        // Assert
        var output = _stringWriter.ToString();
        output.Should().Contain("Sig 1");
        output.Should().Contain("Noise 1");
        output.Should().Contain("Sig 2");
        output.Should().NotContain("hidden"); // No markers
        output.Should().Contain("Showing all 3 activities.");
    }

    private void SetupUseCase(string sessionId, List<SessionActivity> history)
    {
         _useCaseMock.Setup(x => x.ExecuteAsync(It.Is<BrowseHistoryRequest>(r => r.Id.Value == sessionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BrowseHistoryResponse(new SessionId(sessionId), history));
    }
}
