using Cleo.Cli.Commands;
using Cleo.Core.UseCases.BrowseSources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

public class SourcesCommandTests
{
    private readonly SourcesCommand _command;

    public SourcesCommandTests()
    {
        var useCaseMock = new Mock<IBrowseSourcesUseCase>();
        var loggerMock = new Mock<ILogger<SourcesCommand>>();

        _command = new SourcesCommand(useCaseMock.Object, loggerMock.Object);
    }

    [Fact(DisplayName = "Given the sources command, when viewing help, then it should use the term 'GitHub repositories for collaboration'.")]
    public void Build_ShouldUseGitHubRepositoriesTerm()
    {
        // Act
        var command = _command.Build();

        // Assert
        command.Description.Should().Be("List available GitHub repositories for collaboration 🛰️");
    }
}
