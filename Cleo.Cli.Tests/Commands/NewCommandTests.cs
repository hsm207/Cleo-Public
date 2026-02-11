using Cleo.Cli.Commands;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.InitiateSession;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public class NewCommandTests : IDisposable
{
    private readonly Mock<IJulesSessionClient> _julesClientMock;
    private readonly Mock<ISessionWriter> _sessionWriterMock;
    private readonly Mock<ILogger<NewCommand>> _loggerMock;
    private readonly NewCommand _command;
    private readonly StringWriter _stringWriter;
    private readonly TextWriter _originalOutput;

    public NewCommandTests()
    {
        _julesClientMock = new Mock<IJulesSessionClient>();
        _sessionWriterMock = new Mock<ISessionWriter>();

        // We test the command using the real use case logic, but with mocked infrastructure ports.
        // This is a "Sociable Unit Test" of the Command + Use Case layer.
        var useCase = new InitiateSessionUseCase(_julesClientMock.Object, _sessionWriterMock.Object);
        _loggerMock = new Mock<ILogger<NewCommand>>();

        _command = new NewCommand(useCase, _loggerMock.Object);

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

    [Fact(DisplayName = "Given a task, when running 'new', then it should initiate a session and display the ID.")]
    public async Task New_Valid_InitiatesSession()
    {
        // Arrange
        var sessionId = new SessionId("session-123");
        var dashboardUri = new Uri("https://portal.jules.ai/123");
        var createdSession = new Cleo.Core.Domain.Entities.Session(
            sessionId, "remote-1", new TaskDescription("Do thing"), new SourceContext("repo", "main"),
            new SessionPulse(SessionStatus.StartingUp), DateTimeOffset.UtcNow, dashboardUri: dashboardUri);

        _julesClientMock.Setup(x => x.CreateSessionAsync(It.IsAny<TaskDescription>(), It.IsAny<SourceContext>(), It.IsAny<SessionCreationOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdSession);

        // Act
        var exitCode = await _command.Build().InvokeAsync("new \"Do the thing\" --repo my-repo");

        // Assert
        exitCode.Should().Be(0);
        var output = _stringWriter.ToString();
        output.Should().Contain("✨ Session initiated successfully!");
        output.Should().Contain("session-123");
        output.Should().Contain("https://portal.jules.ai/123");

        // Verify persistence was called
        _sessionWriterMock.Verify(x => x.RememberAsync(It.Is<Cleo.Core.Domain.Entities.Session>(s => s.Id == sessionId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Given a custom title, when running 'new', then it should use that title in creation options.")]
    public async Task New_WithTitle_UsesTitle()
    {
        // Arrange
        var sessionId = new SessionId("session-title");
        var createdSession = new Cleo.Core.Domain.Entities.Session(
            sessionId, "remote-1", new TaskDescription("Task"), new SourceContext("repo", "main"),
            new SessionPulse(SessionStatus.StartingUp), DateTimeOffset.UtcNow);

        _julesClientMock.Setup(x => x.CreateSessionAsync(It.IsAny<TaskDescription>(), It.IsAny<SourceContext>(), It.Is<SessionCreationOptions>(o => o.Title == "My Title"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdSession);

        // Act
        await _command.Build().InvokeAsync("new \"Task\" --repo r -t \"My Title\"");

        // Assert
        _julesClientMock.VerifyAll();
    }

    [Fact(DisplayName = "Given an error during creation, when running 'new', then it should handle the exception.")]
    public async Task New_Error_HandlesException()
    {
        // Arrange
        // Note: NewCommand is wired to InitiateSessionUseCase, which creates a SourceContext.
        // SourceContext throws if repo is empty. When we run "new 'Fail'", repo option is missing (null/empty).
        // InitiateSessionUseCase validates request params. If we want to test the *Command's* exception handling of the UseCase throwing,
        // we must provide valid args to get past early validation, OR we rely on the UseCase throwing due to invalid args.
        // In the previous run, "Repository name cannot be empty" was thrown by SourceContext constructor inside UseCase.
        // This *is* an exception handled by the catch block! But my assertion expected "API Error".
        // Let's provide a valid repo so we hit the Mock.

        _julesClientMock.Setup(x => x.CreateSessionAsync(It.IsAny<TaskDescription>(), It.IsAny<SourceContext>(), It.IsAny<SessionCreationOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API Error"));

        // Act
        var exitCode = await _command.Build().InvokeAsync("new \"Fail\" --repo r");

        // Assert
        exitCode.Should().Be(0); // Handled
        _stringWriter.ToString().Should().Contain("💔 Error: API Error");

        _loggerMock.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
    }
}
