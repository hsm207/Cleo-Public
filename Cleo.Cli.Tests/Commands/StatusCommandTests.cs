using Cleo.Cli.Commands;
using Cleo.Core.UseCases.RefreshPulse;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

public class StatusCommandTests
{
    private readonly StatusCommand _command;

    public StatusCommandTests()
    {
        var useCaseMock = new Mock<IRefreshPulseUseCase>();
        var loggerMock = new Mock<ILogger<StatusCommand>>();

        _command = new StatusCommand(useCaseMock.Object, loggerMock.Object);
    }

    [Fact(DisplayName = "Given the status command, when viewing help, then it should use the term 'Pulse' and 'Stance' and 'sessionId'.")]
    public void Build_ShouldUsePulseAndStanceTermAndSessionId()
    {
        // Act
        var command = _command.Build();

        // Assert
        command.Description.Should().Be("Check the Pulse and Stance of a session 💓");

        var argument = command.Arguments.FirstOrDefault();
        argument.Should().NotBeNull();
        argument!.Name.Should().Be("sessionId");
        argument.Description.Should().Be("The session ID.");
    }
}
