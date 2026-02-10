using Cleo.Cli.Commands;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.InitiateSession;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

public class NewCommandTests
{
    private readonly NewCommand _command;

    public NewCommandTests()
    {
        var julesClientMock = new Mock<IJulesSessionClient>();
        var sessionWriterMock = new Mock<ISessionWriter>();
        var useCase = new InitiateSessionUseCase(julesClientMock.Object, sessionWriterMock.Object);
        var loggerMock = new Mock<ILogger<NewCommand>>();

        _command = new NewCommand(useCase, loggerMock.Object);
    }

    [Fact(DisplayName = "Given the new command, when viewing help, then it should use the term 'task' for the primary argument.")]
    public void Build_ShouldUseTaskTermForPrimaryArgument()
    {
        // Act
        var command = _command.Build();

        // Assert
        command.Description.Should().Be("Assign a specific task to the agent. This initiates the collaboration session.");

        var argument = command.Arguments.FirstOrDefault();
        argument.Should().NotBeNull();
        argument!.Name.Should().Be("task");
        argument.Description.Should().Be("The high-level goal or task for Jules.");
    }
}
