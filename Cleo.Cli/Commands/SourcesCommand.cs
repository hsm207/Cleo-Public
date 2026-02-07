using System.CommandLine;
using Cleo.Core.Domain.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

internal static class SourcesCommand
{
    public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new Command("sources", "List available sources in Jules 🌍");

        command.SetHandler(async () =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("SourcesCommand");
            var julesClient = serviceProvider.GetRequiredService<IJulesClient>();

            try
            {
                var sources = await julesClient.ListSourcesAsync().ConfigureAwait(false);

                if (sources.Count == 0)
                {
                    Console.WriteLine("📭 No sources found. Have you connected your GitHub account to Jules? 💖");
                    return;
                }

                Console.WriteLine("📡 Available Sources:");
                foreach (var source in sources)
                {
                    Console.WriteLine($"- {source.Name} ({source.Owner}/{source.Repo})");
                }
            }
            #pragma warning disable CA1031
            catch (Exception ex)
            {
                #pragma warning disable CA1848
                logger.LogError(ex, "❌ Failed to list sources.");
                #pragma warning restore CA1848
                Console.WriteLine($"💔 Something went wrong: {ex.Message}");
            }
            #pragma warning restore CA1031
        });

        return command;
    }
}
