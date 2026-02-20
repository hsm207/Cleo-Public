using Cleo.Cli.Commands;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.BrowseHistory;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public sealed class LogCommandTests
{
    private readonly Mock<IBrowseHistoryUseCase> _useCaseMock;
    private readonly Mock<IStatusPresenter> _presenterMock;
    private readonly Mock<IHelpProvider> _helpProviderMock;
    private readonly LogCommand _command;

    public LogCommandTests()
    {
        _useCaseMock = new Mock<IBrowseHistoryUseCase>();
        _presenterMock = new Mock<IStatusPresenter>();
        _helpProviderMock = new Mock<IHelpProvider>();

        // Fix mocks
        _helpProviderMock.Setup(x => x.GetResource(It.IsAny<string>())).Returns<string>(key =>
            key switch {
                "Cmd_Log_Name" => "log",
                "Cmd_View_Name" => "view",
                "Arg_SessionId_Name" => "sessionId",
                "Opt_All_Aliases" => "--all",
                "Opt_Limit_Aliases" => "--limit",
                _ => key
            });
        _helpProviderMock.Setup(x => x.GetCommandDescription(It.IsAny<string>())).Returns<string>(k => k);

        _command = new LogCommand(
            _useCaseMock.Object,
            _presenterMock.Object,
            _helpProviderMock.Object,
            new Mock<ILogger<LogCommand>>().Object);
    }

    [Fact(DisplayName = "Log view should call UseCase and PresentActivityLog.")]
    public async Task Log_View_PresentsLog()
    {
        // Arrange
        var sessionId = new SessionId("sessions/123");
        var history = new[] { new ProgressActivity("act-1", "rem-1", DateTimeOffset.UtcNow, ActivityOriginator.System, "test") };

        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<BrowseHistoryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BrowseHistoryResponse(sessionId, history, null));

        // Act
        await _command.Build().InvokeAsync($"log view {sessionId}");

        // Assert
        _presenterMock.Verify(x => x.PresentActivityLog(
            sessionId.ToString(),
            It.Is<IEnumerable<SessionActivity>>(l => l.Count() == 1),
            false,
            null,
            null), Times.Once);
    }

    [Fact(DisplayName = "Log view should call PresentEmptyLog when history is empty.")]
    public async Task Log_Empty_PresentsEmpty()
    {
        // Arrange
        var sessionId = new SessionId("sessions/123");
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<BrowseHistoryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BrowseHistoryResponse(sessionId, Array.Empty<SessionActivity>(), null));

        // Act
        await _command.Build().InvokeAsync($"log view {sessionId}");

        // Assert
        _presenterMock.Verify(x => x.PresentEmptyLog(), Times.Once);
    }

    [Fact(DisplayName = "Log view should call PresentError on exception.")]
    public async Task Log_Error_PresentsError()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<BrowseHistoryRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Fail"));

        // Act
        await _command.Build().InvokeAsync($"log view sessions/123");

        // Assert
        _presenterMock.Verify(x => x.PresentError("Fail"), Times.Once);
    }
}
