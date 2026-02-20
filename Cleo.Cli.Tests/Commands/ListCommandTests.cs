using Cleo.Cli.Commands;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ListSessions;
using Cleo.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public sealed class ListCommandTests
{
    private readonly Mock<IListSessionsUseCase> _useCaseMock;
    private readonly Mock<IStatusPresenter> _presenterMock;
    private readonly Mock<IHelpProvider> _helpProviderMock;
    private readonly Mock<ISessionStatusEvaluator> _evaluatorMock;
    private readonly ListCommand _command;

    public ListCommandTests()
    {
        _useCaseMock = new Mock<IListSessionsUseCase>();
        _presenterMock = new Mock<IStatusPresenter>();
        _helpProviderMock = new Mock<IHelpProvider>();
        _evaluatorMock = new Mock<ISessionStatusEvaluator>();

        // Fix mocks
        _helpProviderMock.Setup(x => x.GetResource(It.IsAny<string>())).Returns<string>(key =>
            key switch {
                "Cmd_List_Name" => "list",
                _ => key
            });
        _helpProviderMock.Setup(x => x.GetCommandDescription(It.IsAny<string>())).Returns<string>(k => k);

        _command = new ListCommand(
            _useCaseMock.Object,
            _presenterMock.Object,
            _helpProviderMock.Object,
            _evaluatorMock.Object,
            new Mock<ILogger<ListCommand>>().Object);
    }

    [Fact(DisplayName = "Given no sessions, list command should present empty list.")]
    public async Task List_Empty_PresentsEmpty()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ListSessionsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListSessionsResponse(new List<Cleo.Core.Domain.Entities.Session>()));

        // Act
        await _command.Build().InvokeAsync("list");

        // Assert
        _presenterMock.Verify(x => x.PresentEmptyList(), Times.Once);
    }

    [Fact(DisplayName = "Given sessions, list command should present list.")]
    public async Task List_WithSessions_PresentsList()
    {
        // Arrange
        var session = new Cleo.Core.Domain.Entities.Session(
            TestFactory.CreateSessionId("s1"),
            "r1",
            new TaskDescription("Task"),
            TestFactory.CreateSourceContext("repo"),
            new SessionPulse(SessionStatus.InProgress),
            DateTimeOffset.UtcNow);

        var sessions = new List<Cleo.Core.Domain.Entities.Session> { session };
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ListSessionsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListSessionsResponse(sessions));

        _evaluatorMock.Setup(x => x.Evaluate(It.IsAny<Cleo.Core.UseCases.RefreshPulse.RefreshPulseResponse>()))
            .Returns(new Cleo.Cli.Models.StatusViewModel("State", "PR", "Time", "Head", null, new List<string>().AsReadOnly(), new List<string>().AsReadOnly()));

        // Act
        await _command.Build().InvokeAsync("list");

        // Assert
        _presenterMock.Verify(x => x.PresentSessionList(It.IsAny<IEnumerable<(string, string, string)>>()), Times.Once);
    }

    [Fact(DisplayName = "List command handles exception.")]
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
