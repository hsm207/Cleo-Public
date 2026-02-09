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
        var command = new Command("status", "Check the Pulse and Stance of a session 💓");

        var sessionIdArgument = new Argument<string>("sessionId", "The session ID.");
        command.AddArgument(sessionIdArgument);

        command.SetHandler(async (sessionId) => await ExecuteAsync(sessionId), sessionIdArgument);

        return command;
    }

    private async Task ExecuteAsync(string sessionId)
    {
        try
        {
            var id = new SessionId(sessionId);
            var request = new RefreshPulseRequest(id);
            var response = await _useCase.ExecuteAsync(request).ConfigureAwait(false);

            if (response.Warning != null)
            {
                Console.WriteLine(response.Warning);
            }

            Console.WriteLine($"🧘‍♀️ Stance: {response.Stance}");
            Console.WriteLine($"🏆 Delivery: {response.DeliveryStatus}");

            if (response.PullRequest != null)
            {
                Console.WriteLine($"🎁 Pull Request: {response.PullRequest.Url}");
            }

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
