using Cleo.Cli.Commands;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases;
using Cleo.Core.UseCases.InitiateSession;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public class SessionCommandTests
{
    private readonly SessionCommand _command;

    public SessionCommandTests()
    {
        var julesClientMock = new Mock<IJulesSessionClient>();
        var sessionWriterMock = new Mock<ISessionWriter>();
        var initiateUseCase = new InitiateSessionUseCase(julesClientMock.Object, sessionWriterMock.Object);
        var presenterMock = new Mock<IStatusPresenter>();
        var helpProviderMock = new Mock<IHelpProvider>();

        // Mock help provider for NewCommand and SessionCommand
        helpProviderMock.Setup(x => x.GetCommandDescription(It.IsAny<string>())).Returns<string>(k => k == "Session_Description" ? "Lifecycle Management" : k);

        var newCommand = new NewCommand(initiateUseCase, presenterMock.Object, helpProviderMock.Object, new Mock<ILogger<NewCommand>>().Object);

        var listCommand = new ListCommand(new Mock<Core.UseCases.ListSessions.IListSessionsUseCase>().Object, presenterMock.Object, helpProviderMock.Object, new Mock<ILogger<ListCommand>>().Object);
        var statusCommand = new CheckinCommand(
            new Mock<Core.UseCases.RefreshPulse.IRefreshPulseUseCase>().Object, 
            presenterMock.Object,
            new Mock<ILogger<CheckinCommand>>().Object);
        var forgetCommand = new ForgetCommand(new Mock<Core.UseCases.ForgetSession.IForgetSessionUseCase>().Object, presenterMock.Object, helpProviderMock.Object, new Mock<ILogger<ForgetCommand>>().Object);

        _command = new SessionCommand(newCommand, listCommand, statusCommand, forgetCommand, helpProviderMock.Object);
    }

    [Fact(DisplayName = "Given the Session command, when built, then it should contain all required subcommands.")]
    public void Build_ConstructsHierarchyCorrectly()
    {
        // Act
        var root = _command.Build();

        // Assert
        root.Name.Should().Be("session");
        root.Description.Should().Contain("Lifecycle Management");

        root.Subcommands.Should().Contain(c => c.Name == "new");
        root.Subcommands.Should().Contain(c => c.Name == "list");
        root.Subcommands.Should().Contain(c => c.Name == "checkin");
        root.Subcommands.Should().Contain(c => c.Name == "forget");
    }
}
