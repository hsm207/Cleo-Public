using Cleo.Cli.Commands;
using Cleo.Core.UseCases.ForgetSession;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

public class ForgetCommandTests
{
    private readonly ForgetCommand _command;

    public ForgetCommandTests()
    {
        var useCaseMock = new Mock<IForgetSessionUseCase>();
        var loggerMock = new Mock<ILogger<ForgetCommand>>();

        _command = new ForgetCommand(useCaseMock.Object, loggerMock.Object);
    }

    [Fact(DisplayName = "Given the forget command, when viewing help, then it should use the term 'Session Registry' and 'sessionId'.")]
    public void Build_ShouldUseSessionRegistryTermAndSessionId()
    {
        // Act
        var command = _command.Build();

        // Assert
        command.Description.Should().Be("Forget a session from the local Session Registry 🧹");

        var argument = command.Arguments.FirstOrDefault();
        argument.Should().NotBeNull();
        argument!.Name.Should().Be("sessionId");
        argument.Description.Should().Be("The session ID.");
    }
}
