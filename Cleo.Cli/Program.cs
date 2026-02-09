using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Infrastructure;
using Cleo.Cli.Commands;
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

        // CLI Commands (View Layer) 🖥️
        services.AddTransient<AuthCommand>();
        services.AddTransient<ListCommand>();
        services.AddTransient<NewCommand>();
        services.AddTransient<StatusCommand>();
        services.AddTransient<SourcesCommand>();
        services.AddTransient<TalkCommand>();
        services.AddTransient<ActivitiesCommand>();
        services.AddTransient<ApproveCommand>();
        services.AddTransient<ForgetCommand>();
    }

    private static RootCommand BuildRootCommand(IServiceProvider sp)
    {
        var rootCommand = new RootCommand("🏛️ Cleo: The God-Tier Engineering Assistant")
        {
            sp.GetRequiredService<AuthCommand>().Build(),
            sp.GetRequiredService<ListCommand>().Build(),
            sp.GetRequiredService<NewCommand>().Build(),
            sp.GetRequiredService<StatusCommand>().Build(),
            sp.GetRequiredService<SourcesCommand>().Build(),
            sp.GetRequiredService<TalkCommand>().Build(),
            sp.GetRequiredService<ActivitiesCommand>().Build(),
            sp.GetRequiredService<ApproveCommand>().Build(),
            sp.GetRequiredService<ForgetCommand>().Build()
        };

        return rootCommand;
    }
}
