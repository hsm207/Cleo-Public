using Cleo.Cli.Commands;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
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
        var authCommand = new AuthCommand(
            new Mock<Core.UseCases.AuthenticateUser.IAuthenticateUserUseCase>().Object,
            new Mock<Core.Domain.Ports.IVault>().Object,
            new Mock<IStatusPresenter>().Object,
            new Mock<IHelpProvider>().Object,
            new Mock<ILogger<AuthCommand>>().Object);

        var reposCommand = new ReposCommand(
            new Mock<IBrowseSourcesUseCase>().Object,
            new Mock<IStatusPresenter>().Object,
            new Mock<IHelpProvider>().Object,
            new Mock<ILogger<ReposCommand>>().Object);

        _command = new ConfigCommand(authCommand, reposCommand);
    }

    [Fact(DisplayName = "Config command should contain auth and repos subcommands.")]
    public void Build_ConstructsHierarchyCorrectly()
    {
        // Act
        var root = _command.Build();

        // Assert
        root.Name.Should().Be("config");
        root.Subcommands.Should().Contain(c => c.Name == "auth");
        root.Subcommands.Should().Contain(c => c.Name == "repos");
    }
}
