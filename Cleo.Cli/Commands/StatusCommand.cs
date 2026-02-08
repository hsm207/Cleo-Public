using System.CommandLine;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.RefreshPulse;
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
            var useCase = serviceProvider.GetRequiredService<IRefreshPulseUseCase>();

            try
            {
                var sessionId = new SessionId(handle);
                var request = new RefreshPulseRequest(sessionId);
                var response = await useCase.ExecuteAsync(request).ConfigureAwait(false);

                if (response.IsCached)
                {
                    Console.WriteLine(response.Warning);
                }

                Console.WriteLine($"💓 Status for {handle}: {response.Pulse.Status}");
                Console.WriteLine($"📝 {response.Pulse.Detail}");
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
