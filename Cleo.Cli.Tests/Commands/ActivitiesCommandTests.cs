using Cleo.Cli.Commands;
using Cleo.Core.UseCases.BrowseHistory;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

public class ActivitiesCommandTests
{
    private readonly ActivitiesCommand _command;

    public ActivitiesCommandTests()
    {
        var useCaseMock = new Mock<IBrowseHistoryUseCase>();
        var loggerMock = new Mock<ILogger<ActivitiesCommand>>();

        _command = new ActivitiesCommand(useCaseMock.Object, loggerMock.Object);
    }

    [Fact(DisplayName = "Given the activities command, when viewing help, then it should use the term 'Session Log' and 'sessionId'.")]
    public void Build_ShouldUseSessionLogTermAndSessionId()
    {
        // Act
        var command = _command.Build();

        // Assert
        command.Description.Should().Be("View the Session Log for a session 📜");

        var argument = command.Arguments.FirstOrDefault();
        argument.Should().NotBeNull();
        argument!.Name.Should().Be("sessionId");
        argument.Description.Should().Be("The session ID.");
    }
}
