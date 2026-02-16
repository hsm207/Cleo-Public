using System.CommandLine;
using Cleo.Cli;
using Cleo.Cli.Commands;
using Cleo.Cli.Presenters;
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
public class ProgramTests : IDisposable
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
        var sessionCmd = sp.GetService<SessionCommand>();
        Assert.NotNull(sessionCmd);

        var configCmd = sp.GetService<ConfigCommand>();
        Assert.NotNull(configCmd);
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

        // 1. Register Real Commands (Leafs & Groups)
        services.AddTransient<AuthCommand>();
        services.AddTransient<ListCommand>();
        services.AddTransient<NewCommand>();
        services.AddTransient<CheckinCommand>();
        services.AddTransient<ReposCommand>();
        services.AddTransient<TalkCommand>();
        services.AddTransient<ApproveCommand>();
        services.AddTransient<ForgetCommand>();

        services.AddTransient<SessionCommand>();
        services.AddTransient<LogCommand>();
        services.AddTransient<PlanCommand>();
        services.AddTransient<ConfigCommand>();

        // 3. Register Mocked Dependencies (Infrastructure Ports & Use Cases)
        services.AddLogging();
        services.AddTransient(_ => new Mock<IAuthenticateUserUseCase>().Object);
        services.AddTransient(_ => new Mock<IVault>().Object);
        services.AddTransient(_ => new Mock<IListSessionsUseCase>().Object);
        services.AddTransient(_ => new Mock<IRefreshPulseUseCase>().Object);
        services.AddTransient(_ => new Mock<IBrowseSourcesUseCase>().Object);
        services.AddTransient(_ => new Mock<ICorrespondUseCase>().Object);
        services.AddTransient(_ => new Mock<IApprovePlanUseCase>().Object);
        services.AddTransient(_ => new Mock<IForgetSessionUseCase>().Object);
        services.AddTransient(_ => new Mock<IBrowseHistoryUseCase>().Object);
        services.AddTransient(_ => new Mock<IViewPlanUseCase>().Object);
        services.AddTransient(_ => new Mock<IStatusPresenter>().Object);

        // Special handling for concrete use cases that are dependencies
        services.AddTransient(_ => new InitiateSessionUseCase(new Mock<IJulesSessionClient>().Object, new Mock<ISessionWriter>().Object));

        var sp = services.BuildServiceProvider();

        // Act
        var root = Program.BuildRootCommand(sp);

        // Assert
        Assert.NotNull(root);
        Assert.Contains(root.Children, c => c.Name == "session");
        Assert.Contains(root.Children, c => c.Name == "log");
        Assert.Contains(root.Children, c => c.Name == "plan");
        Assert.Contains(root.Children, c => c.Name == "talk");
        Assert.Contains(root.Children, c => c.Name == "config");
    }
}
