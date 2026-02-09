using Cleo.Cli.Commands;
using Cleo.Core.UseCases.ApprovePlan;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

public class ApproveCommandTests
{
    private readonly ApproveCommand _command;

    public ApproveCommandTests()
    {
        var useCaseMock = new Mock<IApprovePlanUseCase>();
        var loggerMock = new Mock<ILogger<ApproveCommand>>();

        _command = new ApproveCommand(useCaseMock.Object, loggerMock.Object);
    }

    [Fact(DisplayName = "Given the approve command, when viewing help, then it should use the term 'sessionId'.")]
    public void Build_ShouldUseSessionIdTerm()
    {
        // Act
        var command = _command.Build();

        // Assert
        command.Description.Should().Be("Approve a generated plan 👍");

        var argument = command.Arguments.FirstOrDefault();
        argument.Should().NotBeNull();
        argument!.Name.Should().Be("sessionId");
        argument.Description.Should().Be("The session ID.");
    }
}
