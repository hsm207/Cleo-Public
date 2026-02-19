using Cleo.Cli.Commands;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.ListSessions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public class ListCommandTests
{
    private readonly Mock<IListSessionsUseCase> _useCaseMock;
    private readonly Mock<IStatusPresenter> _presenterMock;
    private readonly Mock<IHelpProvider> _helpProviderMock;
    private readonly ListCommand _command;

    public ListCommandTests()
    {
        _useCaseMock = new Mock<IListSessionsUseCase>();
        _presenterMock = new Mock<IStatusPresenter>();
        _helpProviderMock = new Mock<IHelpProvider>();

        _helpProviderMock.Setup(x => x.GetCommandDescription(It.IsAny<string>())).Returns<string>(k => k);

        _command = new ListCommand(
            _useCaseMock.Object,
            _presenterMock.Object,
            _helpProviderMock.Object,
            new Mock<ILogger<ListCommand>>().Object);
    }

    [Fact(DisplayName = "List should call UseCase and PresentSessionList.")]
    public async Task List_Valid_PresentsList()
    {
        // Arrange
        var sessions = new[] {
            new Cleo.Core.Domain.Entities.Session(
                new Cleo.Core.Domain.ValueObjects.SessionId("sessions/1"),
                "rem",
                new Cleo.Core.Domain.ValueObjects.TaskDescription("Task"),
                new Cleo.Core.Domain.ValueObjects.SourceContext("sources/repo", "main"),
                new Cleo.Core.Domain.ValueObjects.SessionPulse(Cleo.Core.Domain.ValueObjects.SessionStatus.InProgress),
                DateTimeOffset.UtcNow)
        };

        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ListSessionsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListSessionsResponse(sessions));

        // Act
        await _command.Build().InvokeAsync("list");

        // Assert
        _presenterMock.Verify(x => x.PresentSessionList(It.Is<IEnumerable<(string, string, string)>>(l => l.Count() == 1)), Times.Once);
    }

    [Fact(DisplayName = "List should call PresentEmptyList when no sessions found.")]
    public async Task List_Empty_PresentsEmpty()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ListSessionsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListSessionsResponse(Array.Empty<Cleo.Core.Domain.Entities.Session>()));

        // Act
        await _command.Build().InvokeAsync("list");

        // Assert
        _presenterMock.Verify(x => x.PresentEmptyList(), Times.Once);
    }

    [Fact(DisplayName = "List should call PresentError on exception.")]
    public async Task List_Error_PresentsError()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ListSessionsRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Fail"));

        // Act
        await _command.Build().InvokeAsync("list");

        // Assert
        _presenterMock.Verify(x => x.PresentError("Fail"), Times.Once);
    }
}
