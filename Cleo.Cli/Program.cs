using System.CommandLine;
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
    }

    private static RootCommand CreateRootCommand(IServiceProvider serviceProvider)
    {
        var rootCommand = new RootCommand("🏛️ Cleo: The God-Tier Engineering Assistant")
        {
            Name = "cleo"
        };

        // Add subcommands ⌨️
        rootCommand.AddCommand(Commands.AuthCommand.Create(serviceProvider));
        rootCommand.AddCommand(Commands.SourcesCommand.Create(serviceProvider));
        rootCommand.AddCommand(Commands.NewCommand.Create(serviceProvider));
        rootCommand.AddCommand(Commands.ListCommand.Create(serviceProvider));
        rootCommand.AddCommand(Commands.StatusCommand.Create(serviceProvider));
        rootCommand.AddCommand(Commands.DeleteCommand.Create(serviceProvider));
        rootCommand.AddCommand(Commands.ActivitiesCommand.Create(serviceProvider));
        rootCommand.AddCommand(Commands.ApproveCommand.Create(serviceProvider));
        rootCommand.AddCommand(Commands.TalkCommand.Create(serviceProvider));
        
        rootCommand.SetHandler(() => 
        {
            Console.WriteLine("🚀 Cleo .NET 10 System Online!");
            Console.WriteLine("Use --help to see available commands, babe! 💖");
        });

        return rootCommand;
    }
}
