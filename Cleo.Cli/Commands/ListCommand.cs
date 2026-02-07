using System.CommandLine;
using Cleo.Core.Domain.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

internal static class ListCommand
{
    public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new Command("list", "List all active missions from the global registry 🌍");

        command.SetHandler(async () =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("ListCommand");
            var reader = serviceProvider.GetRequiredService<ISessionReader>();

            try
            {
                var sessions = await reader.ListAsync().ConfigureAwait(false);

                if (sessions.Count == 0)
                {
                    Console.WriteLine("📭 No active missions found. Time to start something new? 💖");
                    return;
                }

                Console.WriteLine("📋 Current Missions:");
                foreach (var session in sessions)
                {
                    Console.WriteLine($"- [{session.Id.Value}] {session.Task} ({session.Pulse.Status})");
                }
            }
            #pragma warning disable CA1031
            catch (Exception ex)
            {
                #pragma warning disable CA1848
                logger.LogError(ex, "❌ Failed to list sessions.");
                #pragma warning restore CA1848
                Console.WriteLine($"💔 Something went wrong: {ex.Message}");
            }
            #pragma warning restore CA1031
        });

        return command;
    }
}
