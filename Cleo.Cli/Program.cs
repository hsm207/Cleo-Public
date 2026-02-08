using System.CommandLine;
using Cleo.Cli.Commands;
using Cleo.Core.UseCases;
using Cleo.Core.UseCases.AbandonSession;
using Cleo.Core.UseCases.ApprovePlan;
using Cleo.Core.UseCases.AuthenticateUser;
using Cleo.Core.UseCases.BrowseHistory;
using Cleo.Core.UseCases.BrowseSources;
using Cleo.Core.UseCases.Correspond;
using Cleo.Core.UseCases.InitiateSession;
using Cleo.Core.UseCases.ListSessions;
using Cleo.Core.UseCases.RefreshPulse;
using Cleo.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        // 1. Setup DI & Configuration 🏗️
        var services = new ServiceCollection();
        ConfigureServices(services);
        
        using var serviceProvider = services.BuildServiceProvider();
        
        // 2. Setup CLI Commands ⌨️
        var rootCommand = CreateRootCommand(serviceProvider);
        
        // 3. Execute 🚀
        return await rootCommand.InvokeAsync(args).ConfigureAwait(false);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Logging 📜✨
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Infrastructure 🏗️
        var julesBaseUrl = new Uri("https://jules.googleapis.com/");
        services.AddCleoInfrastructure(julesBaseUrl);

        // Use Cases 🧠
        services.AddScoped<IUseCase<InitiateSessionRequest, InitiateSessionResponse>, InitiateSessionUseCase>();
        services.AddScoped<IRefreshPulseUseCase, RefreshPulseUseCase>();
        services.AddScoped<IBrowseHistoryUseCase, BrowseHistoryUseCase>();
        services.AddScoped<IApprovePlanUseCase, ApprovePlanUseCase>();
        services.AddScoped<IAuthenticateUserUseCase, AuthenticateUserUseCase>();
        services.AddScoped<IListSessionsUseCase, ListSessionsUseCase>();
        services.AddScoped<IBrowseSourcesUseCase, BrowseSourcesUseCase>();
        services.AddScoped<IAbandonSessionUseCase, AbandonSessionUseCase>();
        services.AddScoped<ICorrespondUseCase, CorrespondUseCase>();

        // CLI Commands (View Layer) 🖥️
        services.AddTransient<AuthCommand>();
        services.AddTransient<SourcesCommand>();
        services.AddTransient<NewCommand>();
        services.AddTransient<ListCommand>();
        services.AddTransient<StatusCommand>();
        services.AddTransient<DeleteCommand>();
        services.AddTransient<ActivitiesCommand>();
        services.AddTransient<ApproveCommand>();
        services.AddTransient<TalkCommand>();
    }

    private static RootCommand CreateRootCommand(IServiceProvider serviceProvider)
    {
        var rootCommand = new RootCommand("🏛️ Cleo: The God-Tier Engineering Assistant")
        {
            Name = "cleo"
        };

        // Add subcommands resolved via DI 🪄✨
        rootCommand.AddCommand(serviceProvider.GetRequiredService<AuthCommand>().Build());
        rootCommand.AddCommand(serviceProvider.GetRequiredService<SourcesCommand>().Build());
        rootCommand.AddCommand(serviceProvider.GetRequiredService<NewCommand>().Build());
        rootCommand.AddCommand(serviceProvider.GetRequiredService<ListCommand>().Build());
        rootCommand.AddCommand(serviceProvider.GetRequiredService<StatusCommand>().Build());
        rootCommand.AddCommand(serviceProvider.GetRequiredService<DeleteCommand>().Build());
        rootCommand.AddCommand(serviceProvider.GetRequiredService<ActivitiesCommand>().Build());
        rootCommand.AddCommand(serviceProvider.GetRequiredService<ApproveCommand>().Build());
        rootCommand.AddCommand(serviceProvider.GetRequiredService<TalkCommand>().Build());
        
        rootCommand.SetHandler(() => 
        {
            Console.WriteLine("🚀 Cleo .NET 10 System Online!");
            Console.WriteLine("Use --help to see available commands! ✨");
        });

        return rootCommand;
    }
}
