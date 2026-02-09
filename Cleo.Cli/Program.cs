using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Cli.Commands;
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

        // 3. Execute 🚀
        try
        {
            return await rootCommand.InvokeAsync(args).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Fatal Error: {ex.Message}");
            return 1;
        }
    }

    private static void ConfigureServices(IServiceCollection services)
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
        services.AddScoped<Cleo.Core.UseCases.ListSessions.IListSessionsUseCase, Cleo.Core.UseCases.ListSessions.ListSessionsUseCase>();
        services.AddScoped<Cleo.Core.UseCases.BrowseSources.IBrowseSourcesUseCase, Cleo.Core.UseCases.BrowseSources.BrowseSourcesUseCase>();
        services.AddScoped<Cleo.Core.UseCases.Correspond.ICorrespondUseCase, Cleo.Core.UseCases.Correspond.CorrespondUseCase>();
        services.AddScoped<Cleo.Core.UseCases.ForgetSession.IForgetSessionUseCase, Cleo.Core.UseCases.ForgetSession.ForgetSessionUseCase>();
        services.AddScoped<Cleo.Core.UseCases.BrowseHistory.IBrowseHistoryUseCase, Cleo.Core.UseCases.BrowseHistory.BrowseHistoryUseCase>();
        services.AddScoped<Cleo.Core.UseCases.ViewPlan.IViewPlanUseCase, Cleo.Core.UseCases.ViewPlan.ViewPlanUseCase>();

        // CLI Commands (Leafs) 🍃
        services.AddTransient<AuthCommand>();
        services.AddTransient<ListCommand>();
        services.AddTransient<NewCommand>();
        services.AddTransient<StatusCommand>();
        services.AddTransient<ReposCommand>();
        services.AddTransient<TalkCommand>();
        services.AddTransient<ApproveCommand>();
        services.AddTransient<ForgetCommand>();

        // CLI Command Groups 🌳
        services.AddTransient<SessionCommand>();
        services.AddTransient<LogCommand>();
        services.AddTransient<PlanCommand>();
        services.AddTransient<ConfigCommand>();
    }

    private static RootCommand BuildRootCommand(IServiceProvider sp)
    {
        var rootCommand = new RootCommand("🏛️ Cleo: The God-Tier Engineering Assistant")
        {
            sp.GetRequiredService<SessionCommand>().Build(),
            sp.GetRequiredService<LogCommand>().Build(),
            sp.GetRequiredService<PlanCommand>().Build(),
            sp.GetRequiredService<TalkCommand>().Build(),
            sp.GetRequiredService<ConfigCommand>().Build()
        };

        return rootCommand;
    }
}
