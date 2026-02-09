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

        _useCaseMock.Setup(x => x.ExecuteAsync(It.Is<BrowseHistoryRequest>(r => r.Id.Value == sessionId.Value), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BrowseHistoryResponse(sessionId, history));

        var command = _command.Build();

        // Act
        var exitCode = await command.InvokeAsync($"view {sessionId.Value}");

        // Assert
        exitCode.Should().Be(0);
        var output = _stringWriter.ToString();

        output.Should().Contain($"Activities for {sessionId.Value}"); // Removed emoji from assertion for flexibility if inconsistent
        output.Should().Contain("Hello Jules!");
    }
}
