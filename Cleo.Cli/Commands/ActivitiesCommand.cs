using System.CommandLine;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

internal static class ActivitiesCommand
{
    public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new Command("activities", "List recent activities for a session 📜");

        var handleArgument = new Argument<string>("handle", "The session handle (ID).");
        command.AddArgument(handleArgument);

        command.SetHandler(async (handle) =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("ActivitiesCommand");
            var julesClient = serviceProvider.GetRequiredService<IJulesActivityClient>();
            var reader = serviceProvider.GetRequiredService<ISessionReader>();

            try
            {
                var sessionId = new SessionId(handle);
                var session = await reader.GetByIdAsync(sessionId).ConfigureAwait(false);

                if (session == null)
                {
                    Console.WriteLine($"🔍 Handle {handle} not found in the registry, babe. 🥀");
                    return;
                }

                var activities = await julesClient.GetActivitiesAsync(sessionId).ConfigureAwait(false);

                if (activities.Count == 0)
                {
                    Console.WriteLine("📭 No activities found yet. Stay tuned! 📻");
                    return;
                }

                Console.WriteLine($"📜 Activities for {handle}:");
                foreach (var activity in activities)
                {
                    Console.WriteLine($"- [{activity.Timestamp:t}] {activity.GetType().Name} ({activity.Id})");
                }
            }
            #pragma warning disable CA1031
            catch (Exception ex)
            {
                #pragma warning disable CA1848
                logger.LogError(ex, "❌ Failed to fetch activities.");
                #pragma warning restore CA1848
                Console.WriteLine($"💔 Something went wrong: {ex.Message}");
            }
            #pragma warning restore CA1031
        }, handleArgument);

        return command;
    }
}
