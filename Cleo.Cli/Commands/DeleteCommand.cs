using System.CommandLine;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

internal static class DeleteCommand
{
    public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new Command("delete", "Remove a task from the local registry 🗑️");

        var handleArgument = new Argument<string>("handle", "The session handle (ID) to remove.");
        command.AddArgument(handleArgument);

        command.SetHandler(async (handle) =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DeleteCommand");
            var repository = serviceProvider.GetRequiredService<ISessionRepository>();

            try
            {
                var sessionId = new SessionId(handle);
                await repository.DeleteAsync(sessionId).ConfigureAwait(false);

                Console.WriteLine($"🗑️ Session {handle} removed from registry. Goodbye, sweet prince! 🥀");
            }
            #pragma warning disable CA1031
            catch (Exception ex)
            {
                #pragma warning disable CA1848
                logger.LogError(ex, "❌ Failed to delete session.");
                #pragma warning restore CA1848
                Console.WriteLine($"💔 Something went wrong: {ex.Message}");
            }
            #pragma warning restore CA1031
        }, handleArgument);

        return command;
    }
}
