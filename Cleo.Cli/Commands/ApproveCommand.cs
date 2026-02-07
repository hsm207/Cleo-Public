using System.CommandLine;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

internal static class ApproveCommand
{
    public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new Command("approve", "Approve a generated plan 👍");

        var handleArgument = new Argument<string>("handle", "The session handle (ID).");
        var planIdArgument = new Argument<string>("planId", "The ID of the plan to approve.");
        command.AddArgument(handleArgument);
        command.AddArgument(planIdArgument);

        command.SetHandler(async (handle, planId) =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("ApproveCommand");
            var julesClient = serviceProvider.GetRequiredService<IJulesSessionClient>();
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

                // Sending an approval message
                await julesClient.SendMessageAsync(sessionId, $"Plan {planId} approved.").ConfigureAwait(false);

                Console.WriteLine($"✅ Plan {planId} approved for session {handle}! Let's go! 🚀");
            }
            #pragma warning disable CA1031
            catch (Exception ex)
            {
                #pragma warning disable CA1848
                logger.LogError(ex, "❌ Failed to approve plan.");
                #pragma warning restore CA1848
                Console.WriteLine($"💔 Something went wrong: {ex.Message}");
            }
            #pragma warning restore CA1031
        }, handleArgument, planIdArgument);

        return command;
    }
}
