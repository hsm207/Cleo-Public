using System.CommandLine;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

internal static class StatusCommand
{
    public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new Command("status", "Fetch the fresh pulse and update the registry 💓");

        var handleArgument = new Argument<string>("handle", "The session handle (ID).");
        command.AddArgument(handleArgument);

        command.SetHandler(async (handle) =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("StatusCommand");
            var julesClient = serviceProvider.GetRequiredService<IJulesClient>();
            var repository = serviceProvider.GetRequiredService<ISessionRepository>();

            try
            {
                var sessionId = new SessionId(handle);
                var session = await repository.GetByIdAsync(sessionId).ConfigureAwait(false);

                if (session == null)
                {
                    Console.WriteLine($"🔍 Handle {handle} not found in the registry, babe. 🥀");
                    return;
                }

                var pulse = await julesClient.GetSessionPulseAsync(sessionId).ConfigureAwait(false);
                
                // Update session pulse
                session.UpdatePulse(pulse);
                
                await repository.SaveAsync(session).ConfigureAwait(false);

                Console.WriteLine($"💓 Status for {handle}: {session.Pulse.Status}");
                Console.WriteLine($"📝 {session.Pulse.Detail}");
            }
            #pragma warning disable CA1031
            catch (Exception ex)
            {
                #pragma warning disable CA1848
                logger.LogError(ex, "❌ Failed to fetch status.");
                #pragma warning restore CA1848
                Console.WriteLine($"💔 Something went wrong: {ex.Message}");
            }
            #pragma warning restore CA1031
        }, handleArgument);

        return command;
    }
}
