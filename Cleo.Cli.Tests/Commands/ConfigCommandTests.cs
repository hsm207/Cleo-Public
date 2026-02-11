using Cleo.Cli.Commands;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.AuthenticateUser;
using Cleo.Core.UseCases.BrowseSources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public class ConfigCommandTests
{
    private readonly ConfigCommand _command;

    public ConfigCommandTests()
    {
        // Mock subcommands
        var authCommand = new AuthCommand(
            new Mock<IAuthenticateUserUseCase>().Object,
            new Mock<ICredentialStore>().Object,
            new Mock<ILogger<AuthCommand>>().Object
        );
        var reposCommand = new ReposCommand(
            new Mock<IBrowseSourcesUseCase>().Object,
            new Mock<ILogger<ReposCommand>>().Object
        );

        _command = new ConfigCommand(authCommand, reposCommand);
    }

    [Fact(DisplayName = "Given the Config command, when built, then it should contain subcommands for Auth and Repos.")]
    public void Build_ConstructsHierarchyCorrectly()
    {
        // Act
        var root = _command.Build();

        // Assert
        root.Name.Should().Be("config");
        root.Description.Should().Contain("management");

        root.Subcommands.Should().Contain(c => c.Name == "auth");
        root.Subcommands.Should().Contain(c => c.Name == "repos");
    }
}
