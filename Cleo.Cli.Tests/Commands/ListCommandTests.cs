using Cleo.Cli.Commands;
using Cleo.Core.UseCases.ListSessions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

public class ListCommandTests
{
    private readonly ListCommand _command;

    public ListCommandTests()
    {
        var useCaseMock = new Mock<IListSessionsUseCase>();
        var loggerMock = new Mock<ILogger<ListCommand>>();

        _command = new ListCommand(useCaseMock.Object, loggerMock.Object);
    }

    [Fact(DisplayName = "Given the list command, when viewing help, then it should use the term 'Session Registry'.")]
    public void Build_ShouldUseSessionRegistryTerm()
    {
        // Act
        var command = _command.Build();

        // Assert
        command.Description.Should().Be("List sessions in the local Session Registry 📋");
    }
}
