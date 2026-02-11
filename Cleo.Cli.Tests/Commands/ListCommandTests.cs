using Cleo.Cli.Commands;
using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ListSessions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public class ListCommandTests : IDisposable
{
    private readonly Mock<IListSessionsUseCase> _useCaseMock;
    private readonly Mock<ILogger<ListCommand>> _loggerMock;
    private readonly ListCommand _command;
    private readonly StringWriter _stringWriter;
    private readonly TextWriter _originalOutput;

    public ListCommandTests()
    {
        _useCaseMock = new Mock<IListSessionsUseCase>();
        _loggerMock = new Mock<ILogger<ListCommand>>();
        _command = new ListCommand(_useCaseMock.Object, _loggerMock.Object);

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

    [Fact(DisplayName = "Given sessions exist, when running 'list', then it should display them.")]
    public async Task List_WithSessions_DisplaysList()
    {
        // Arrange
        var sessions = new List<Session>
        {
            new Session(new SessionId("s1"), "r1", new TaskDescription("Task 1"), new SourceContext("src", "main"), new SessionPulse(SessionStatus.InProgress), DateTimeOffset.UtcNow)
        };
        var response = new ListSessionsResponse(sessions);

        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ListSessionsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var exitCode = await _command.Build().InvokeAsync("list");

        // Assert
        exitCode.Should().Be(0);
        var output = _stringWriter.ToString();
        output.Should().Contain("s1");
        output.Should().Contain("Task 1");
        output.Should().Contain("InProgress");
    }

    [Fact(DisplayName = "Given no sessions, when running 'list', then it should display a friendly empty message.")]
    public async Task List_NoSessions_DisplaysEmptyMessage()
    {
        // Arrange
        var response = new ListSessionsResponse(new List<Session>());

        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ListSessionsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _command.Build().InvokeAsync("list");

        // Assert
        _stringWriter.ToString().Should().Contain("📭 No active sessions found");
    }

    [Fact(DisplayName = "Given an error, when running 'list', then it should handle the exception.")]
    public async Task List_Error_HandlesException()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ListSessionsRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Disk error"));

        // Act
        await _command.Build().InvokeAsync("list");

        // Assert
        _stringWriter.ToString().Should().Contain("💔 Error: Disk error");
    }
}
