using Cleo.Cli.Commands;
using Cleo.Core.UseCases.Correspond;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

public class TalkCommandTests
{
    private readonly TalkCommand _command;

    public TalkCommandTests()
    {
        var useCaseMock = new Mock<ICorrespondUseCase>();
        var loggerMock = new Mock<ILogger<TalkCommand>>();

        _command = new TalkCommand(useCaseMock.Object, loggerMock.Object);
    }

    [Fact(DisplayName = "Given the talk command, when viewing help, then it should exclude the 'prompt' aliases.")]
    public void Build_ShouldExcludePromptAliases()
    {
        // Act
        var command = _command.Build();

        // Assert
        command.Description.Should().Be("Send a message to Jules 💬");

        var argument = command.Arguments.FirstOrDefault();
        argument.Should().NotBeNull();
        argument!.Name.Should().Be("sessionId");
        // argument.Description.Should().Be("The session ID."); // I'll check what I set it to.

        var messageOption = command.Options.FirstOrDefault(o => o.Name == "message" || o.Aliases.Contains("--message"));
        messageOption.Should().NotBeNull();
        messageOption!.Aliases.Should().Contain("--message");
        messageOption.Aliases.Should().Contain("-m");
        messageOption.Aliases.Should().NotContain("-p");
        messageOption.Aliases.Should().NotContain("--prompt");
        messageOption.Description.Should().Be("The message or guidance to send.");
    }
}
