using Cleo.Cli.Commands;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.InitiateSession;
using Cleo.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public sealed class NewCommandTests : IDisposable
{
    private readonly Mock<IJulesSessionClient> _julesClientMock;
    private readonly Mock<ISessionWriter> _sessionWriterMock;
    private readonly Mock<ILogger<NewCommand>> _loggerMock;
    private readonly Mock<IStatusPresenter> _presenterMock;
    private readonly Mock<IHelpProvider> _helpProviderMock;
    private readonly NewCommand _command;

    public NewCommandTests()
    {
        _julesClientMock = new Mock<IJulesSessionClient>();
        _sessionWriterMock = new Mock<ISessionWriter>();
        _presenterMock = new Mock<IStatusPresenter>();
        _helpProviderMock = new Mock<IHelpProvider>();

        // Setup Help Provider to return key as value for simplicity
        _helpProviderMock.Setup(x => x.GetCommandDescription(It.IsAny<string>()))
            .Returns<string>(key => key);

        // We test the command using the real use case logic, but with mocked infrastructure ports.
        // This is a "Sociable Unit Test" of the Command + Use Case layer.
        var useCase = new InitiateSessionUseCase(_julesClientMock.Object, _sessionWriterMock.Object);
        _loggerMock = new Mock<ILogger<NewCommand>>();

        _command = new NewCommand(useCase, _presenterMock.Object, _helpProviderMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Given a task, when running 'new', then it should initiate a session and display the ID.")]
    public async Task New_Valid_InitiatesSession()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("session-123");
        var dashboardUri = new Uri("https://portal.jules.ai/123");
        var createdSession = new Cleo.Core.Domain.Entities.Session(
            sessionId, "remote-1", new TaskDescription("Do thing"), TestFactory.CreateSourceContext("repo"),
            new SessionPulse(SessionStatus.StartingUp), DateTimeOffset.UtcNow, dashboardUri: dashboardUri);

        _julesClientMock.Setup(x => x.CreateSessionAsync(It.IsAny<TaskDescription>(), It.IsAny<SourceContext>(), It.IsAny<SessionCreationOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdSession);

        // Act
        var exitCode = await _command.Build().InvokeAsync("new \"Do the thing\" --repo sources/my-repo");

        // Assert
        exitCode.Should().Be(0);

        _presenterMock.Verify(x => x.PresentNewSession(
            It.Is<string>(s => s == "sessions/session-123"),
            It.Is<string>(u => u == "https://portal.jules.ai/123")), Times.Once);

        // Verify persistence was called
        _sessionWriterMock.Verify(x => x.RememberAsync(It.Is<Cleo.Core.Domain.Entities.Session>(s => s.Id == sessionId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Given a custom title, when running 'new', then it should use that title in creation options.")]
    public async Task New_WithTitle_UsesTitle()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("session-title");
        var createdSession = new Cleo.Core.Domain.Entities.Session(
            sessionId, "remote-1", new TaskDescription("Task"), TestFactory.CreateSourceContext("repo"),
            new SessionPulse(SessionStatus.StartingUp), DateTimeOffset.UtcNow);

        _julesClientMock.Setup(x => x.CreateSessionAsync(It.IsAny<TaskDescription>(), It.IsAny<SourceContext>(), It.Is<SessionCreationOptions>(o => o.Title == "My Title"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdSession);

        // Act
        await _command.Build().InvokeAsync("new \"Task\" --repo sources/r -t \"My Title\"");

        // Assert
        _julesClientMock.VerifyAll();
    }

    [Fact(DisplayName = "Given an error during creation, when running 'new', then it should handle the exception.")]
    public async Task New_Error_HandlesException()
    {
        // Arrange
        _julesClientMock.Setup(x => x.CreateSessionAsync(It.IsAny<TaskDescription>(), It.IsAny<SourceContext>(), It.IsAny<SessionCreationOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API Error"));

        // Act
        var exitCode = await _command.Build().InvokeAsync("new \"Fail\" --repo sources/r");

        // Assert
        exitCode.Should().Be(0); // Handled
        _presenterMock.Verify(x => x.PresentError("API Error"), Times.Once);

        _loggerMock.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
    }
}
