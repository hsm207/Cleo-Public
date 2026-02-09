using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.RefreshPulse;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class StatusCommand
{
    private readonly IRefreshPulseUseCase _useCase;
    private readonly ILogger<StatusCommand> _logger;

    public StatusCommand(IRefreshPulseUseCase useCase, ILogger<StatusCommand> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("status", "Check the status and heartbeat of a session 💓");

        var handleArgument = new Argument<string>("handle", "The session handle (ID).");
        command.AddArgument(handleArgument);

        command.SetHandler(async (handle) => await ExecuteAsync(handle), handleArgument);

        return command;
    }

    private async Task ExecuteAsync(string handle)
    {
        try
        {
            var sessionId = new SessionId(handle);
            var request = new RefreshPulseRequest(sessionId);
            var response = await _useCase.ExecuteAsync(request).ConfigureAwait(false);

            if (response.Warning != null)
            {
                Console.WriteLine(response.Warning);
            }

            Console.WriteLine($"💓 Status for {handle}: {response.Pulse.Status}");
            Console.WriteLine($"📝 {response.Pulse.Detail}");
        }
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to fetch status.");
            #pragma warning restore CA1848
            Console.WriteLine($"💔 Error: {ex.Message}");
        }
    }
}
