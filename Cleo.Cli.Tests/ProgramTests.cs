using System.CommandLine;
using Cleo.Cli;
using Cleo.Cli.Commands;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.ApprovePlan;
using Cleo.Core.UseCases.AuthenticateUser;
using Cleo.Core.UseCases.BrowseHistory;
using Cleo.Core.UseCases.BrowseSources;
using Cleo.Core.UseCases.Correspond;
using Cleo.Core.UseCases.ForgetSession;
using Cleo.Core.UseCases.InitiateSession;
using Cleo.Core.UseCases.ListSessions;
using Cleo.Core.UseCases.RefreshPulse;
using Cleo.Core.UseCases.ViewPlan;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Cleo.Cli.Tests;

[Collection("ConsoleTests")]
public sealed class ProgramTests : IDisposable
{
    private readonly StringWriter _stringWriter;
    private readonly TextWriter _originalOutput;

    public ProgramTests()
    {
        _stringWriter = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_stringWriter);
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _stringWriter.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "ConfigureServices should register all dependencies successfully.")]
    public void ConfigureServices_RegistersDependencies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Program.ConfigureServices(services);
        using var sp = services.BuildServiceProvider();

        // Assert
        // Try to resolve the Root dependencies to ensure graph is complete
        // ICommandGroup should return multiple services
        var commandGroups = sp.GetServices<ICommandGroup>();
        Assert.NotEmpty(commandGroups);

        Assert.Contains(commandGroups, x => x is SessionCommand);
        Assert.Contains(commandGroups, x => x is LogCommand);
        Assert.Contains(commandGroups, x => x is PlanCommand);
        Assert.Contains(commandGroups, x => x is TalkCommand);
        Assert.Contains(commandGroups, x => x is ConfigCommand);
    }

    [Fact(DisplayName = "Given --help argument, Program.Main should execute and return success (0).")]
    public async Task Main_Help_ReturnsSuccess()
    {
        // Act
        var result = await Program.Main(new[] { "--help" });

        // Debugging
        if (result != 0)
        {
            _originalOutput.WriteLine($"Captured Output from Main failure:\n{_stringWriter}");
        }

        // Assert
        Assert.Equal(0, result);
    }

    [Fact(DisplayName = "BuildRootCommand should wire up all top-level commands.")]
    public void BuildRootCommand_WiresUpCommands()
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
        helpProviderMock.Setup(x => x.GetResource(It.IsAny<string>())).Returns("Root Description");
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
}
