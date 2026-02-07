using System.CommandLine;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

internal static class TalkCommand
{
    private static readonly string[] MessageAliases = { "--message", "-m", "--prompt", "-p" };

    public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new Command("talk", "Send a message or prompt to Jules 💬");

        var handleArgument = new Argument<string>("handle", "The session handle (ID).");
        var messageOption = new Option<string>(MessageAliases, "The message or prompt to send.") { IsRequired = true };

        command.AddArgument(handleArgument);
        command.AddOption(messageOption);

        command.SetHandler(async (handle, message) =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("TalkCommand");
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

                Console.WriteLine($"💬 Sending message to {handle}...");
                await julesClient.SendMessageAsync(sessionId, message).ConfigureAwait(false);

                Console.WriteLine($"✅ Message sent! Jules is thinking... 🤔");
            }
            #pragma warning disable CA1031
            catch (Exception ex)
            {
                #pragma warning disable CA1848
                logger.LogError(ex, "❌ Failed to send message.");
                #pragma warning restore CA1848
                Console.WriteLine($"💔 Something went wrong: {ex.Message}");
            }
            #pragma warning restore CA1031
        }, handleArgument, messageOption);

        return command;
    }
}
