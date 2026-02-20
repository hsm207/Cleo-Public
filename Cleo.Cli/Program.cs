using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Cli.Commands;
using Cleo.Cli.Services;
using Cleo.Cli.Presenters;
using Cleo.Core.UseCases.ViewPlan;
using Cleo.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli;

internal static class Program
{
    private static readonly Uri DefaultJulesBaseUrl = new("https://jules.googleapis.com/");

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Top-level CLI entry point.")]
    public static async Task<int> Main(string[] args)
    {
        // 1. Setup DI & Configuration 🏗️
        var services = new ServiceCollection();
        ConfigureServices(services);

        using var serviceProvider = services.BuildServiceProvider();

        // 2. Setup CLI Commands ⌨️
        var rootCommand = BuildRootCommand(serviceProvider);
        var presenter = serviceProvider.GetRequiredService<IStatusPresenter>();

        // 3. Execute 🚀
        try
        {
            return await rootCommand.InvokeAsync(args).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            presenter.PresentError($"💥 Fatal Error: {ex.Message}");
            return 1;
        }
    }

    internal static void ConfigureServices(IServiceCollection services)
    {
        // Infrastructure 🏗️
        services.AddCleoInfrastructure(DefaultJulesBaseUrl);

        // Logging 🪵
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        // Use Cases 🧠
        services.AddScoped<Cleo.Core.UseCases.InitiateSession.IInitiateSessionUseCase, Cleo.Core.UseCases.InitiateSession.InitiateSessionUseCase>();
        services.AddScoped<Cleo.Core.UseCases.ListSessions.IListSessionsUseCase, Cleo.Core.UseCases.ListSessions.ListSessionsUseCase>();
        services.AddScoped<Cleo.Core.UseCases.BrowseSources.IBrowseSourcesUseCase, Cleo.Core.UseCases.BrowseSources.BrowseSourcesUseCase>();
        services.AddScoped<Cleo.Core.UseCases.Correspond.ICorrespondUseCase, Cleo.Core.UseCases.Correspond.CorrespondUseCase>();
        services.AddScoped<Cleo.Core.UseCases.RefreshPulse.IRefreshPulseUseCase, Cleo.Core.UseCases.RefreshPulse.RefreshPulseUseCase>();
        services.AddScoped<Cleo.Core.UseCases.ForgetSession.IForgetSessionUseCase, Cleo.Core.UseCases.ForgetSession.ForgetSessionUseCase>();
        services.AddScoped<Cleo.Core.UseCases.BrowseHistory.IBrowseHistoryUseCase, Cleo.Core.UseCases.BrowseHistory.BrowseHistoryUseCase>();
        services.AddScoped<Cleo.Core.UseCases.ViewPlan.IViewPlanUseCase, Cleo.Core.UseCases.ViewPlan.ViewPlanUseCase>();

        // CLI Commands (Leafs) 🍃
        services.AddTransient<AuthCommand>();
        services.AddTransient<ListCommand>();
        services.AddTransient<NewCommand>();
        services.AddTransient<CheckinCommand>();
        services.AddTransient<ReposCommand>();
        services.AddTransient<TalkCommand>();
        services.AddTransient<ApproveCommand>();
        services.AddTransient<ForgetCommand>();

        // CLI Services 🛡️
        services.AddSingleton<System.CommandLine.IConsole, System.CommandLine.IO.SystemConsole>();
        services.AddSingleton<Cleo.Cli.Services.IHelpProvider, Cleo.Cli.Services.HelpProvider>();
        services.AddTransient<Cleo.Cli.Services.ISessionStatusEvaluator, Cleo.Cli.Services.SessionStatusEvaluator>();
        services.AddSingleton<Cleo.Cli.Presenters.IStatusPresenter, Cleo.Cli.Presenters.CliStatusPresenter>();

        // CLI Command Groups 🌳
        services.AddTransient<ICommandGroup, SessionCommand>();
        services.AddTransient<ICommandGroup, LogCommand>();
        services.AddTransient<ICommandGroup, PlanCommand>();
        services.AddTransient<ICommandGroup, TalkCommand>();
        services.AddTransient<ICommandGroup, ConfigCommand>();
    }

    internal static RootCommand BuildRootCommand(IServiceProvider sp)
    {
        var helpProvider = sp.GetRequiredService<IHelpProvider>();
        var rootCommand = new RootCommand(helpProvider.GetResource("Root_Description"));

        var commandGroups = sp.GetServices<ICommandGroup>();
        foreach (var group in commandGroups)
        {
            rootCommand.AddCommand(group.Build());
        }

        return rootCommand;
    }
}
