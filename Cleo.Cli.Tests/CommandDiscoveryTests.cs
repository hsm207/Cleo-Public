using System.CommandLine;
using Cleo.Cli;
using Cleo.Cli.Commands;
using Cleo.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Cleo.Cli.Tests;

public sealed class CommandDiscoveryTests
{
    [Fact(DisplayName = "Given registered command groups, when building root command, then all groups should be added.")]
    public void BuildRootCommand_AddsAllGroups()
    {
        // Arrange
        var services = new ServiceCollection();

        // Mock ICommandGroup
        var sessionGroupMock = new Mock<ICommandGroup>();
        sessionGroupMock.Setup(x => x.Build()).Returns(new Command("session"));

        var logGroupMock = new Mock<ICommandGroup>();
        logGroupMock.Setup(x => x.Build()).Returns(new Command("log"));

        services.AddSingleton(sessionGroupMock.Object);
        services.AddSingleton(logGroupMock.Object);

        // Mock HelpProvider
        var helpProviderMock = new Mock<IHelpProvider>();
        helpProviderMock.Setup(x => x.GetResource("Root_Description")).Returns("Root Description");
        services.AddSingleton(helpProviderMock.Object);

        var sp = services.BuildServiceProvider();

        // Act
        var root = Program.BuildRootCommand(sp);

        // Assert
        Assert.NotNull(root);
        Assert.Contains(root.Children, c => c.Name == "session");
        Assert.Contains(root.Children, c => c.Name == "log");
        Assert.Equal("Root Description", root.Description);
    }

    [Fact(DisplayName = "ConfigureServices should register all expected command groups.")]
    public void ConfigureServices_RegistersCommandGroups()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Program.ConfigureServices(services);
        using var sp = services.BuildServiceProvider();

        // Assert
        var commandGroups = sp.GetServices<ICommandGroup>();
        Assert.NotEmpty(commandGroups);

        Assert.Contains(commandGroups, x => x is SessionCommand);
        Assert.Contains(commandGroups, x => x is LogCommand);
        Assert.Contains(commandGroups, x => x is PlanCommand);
        Assert.Contains(commandGroups, x => x is TalkCommand);
        Assert.Contains(commandGroups, x => x is ConfigCommand);
    }
}
