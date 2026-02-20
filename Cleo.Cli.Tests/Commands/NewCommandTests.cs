using Cleo.Cli.Commands;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
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
    private readonly Mock<IInitiateSessionUseCase> _useCaseMock;
    private readonly Mock<ILogger<NewCommand>> _loggerMock;
    private readonly Mock<IStatusPresenter> _presenterMock;
    private readonly Mock<IHelpProvider> _helpProviderMock;
    private readonly NewCommand _command;

    public NewCommandTests()
    {
        _useCaseMock = new Mock<IInitiateSessionUseCase>();
        _presenterMock = new Mock<IStatusPresenter>();
        _helpProviderMock = new Mock<IHelpProvider>();

        // Setup Help Provider to return key as value for simplicity
        _helpProviderMock.Setup(x => x.GetCommandDescription(It.IsAny<string>()))
            .Returns<string>(key => key);

        _loggerMock = new Mock<ILogger<NewCommand>>();

        _command = new NewCommand(_useCaseMock.Object, _presenterMock.Object, _helpProviderMock.Object, _loggerMock.Object);
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

        var response = new InitiateSessionResponse(sessionId, dashboardUri, true);

        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<InitiateSessionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var exitCode = await _command.Build().InvokeAsync("new \"Do the thing\" --repo sources/my-repo");

        // Assert
        exitCode.Should().Be(0);

        _presenterMock.Verify(x => x.PresentNewSession(
            It.Is<string>(s => s == "sessions/session-123"),
            It.Is<string>(u => u == "https://portal.jules.ai/123")), Times.Once);

        _useCaseMock.Verify(x => x.ExecuteAsync(
            It.Is<InitiateSessionRequest>(r => r.TaskDescription == "Do the thing" && r.RepoContext == "sources/my-repo"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Given a custom title, when running 'new', then it should use that title in creation options.")]
    public async Task New_WithTitle_UsesTitle()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("session-title");
        var response = new InitiateSessionResponse(sessionId, null, true);

        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<InitiateSessionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _command.Build().InvokeAsync("new \"Task\" --repo sources/r -t \"My Title\"");

        // Assert
        _useCaseMock.Verify(x => x.ExecuteAsync(
            It.Is<InitiateSessionRequest>(r => r.UserProvidedTitle == "My Title"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Given an error during creation, when running 'new', then it should handle the exception.")]
    public async Task New_Error_HandlesException()
    {
        // Arrange
        var exception = new Exception("API Error");
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<InitiateSessionRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var exitCode = await _command.Build().InvokeAsync("new \"Fail\" --repo sources/r");

        // Assert
        exitCode.Should().Be(0); // Handled
        _presenterMock.Verify(x => x.PresentError("API Error"), Times.Once);

        _loggerMock.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
    }
}
