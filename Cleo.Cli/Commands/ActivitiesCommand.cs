using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.BrowseHistory;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class ActivitiesCommand
{
    private readonly IBrowseHistoryUseCase _useCase;
    private readonly ILogger<ActivitiesCommand> _logger;

    public ActivitiesCommand(IBrowseHistoryUseCase useCase, ILogger<ActivitiesCommand> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("activities", "List recent activities for a session 📜");

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
            var request = new BrowseHistoryRequest(sessionId);
            var response = await _useCase.ExecuteAsync(request).ConfigureAwait(false);

            if (response.History.Count == 0)
            {
                Console.WriteLine("📭 No activities found yet. Stay tuned! 📻");
                return;
            }

            Console.WriteLine($"📜 Activities for {handle}:");
            foreach (var activity in response.History)
            {
                Console.WriteLine($"- [{activity.Timestamp:t}] {activity.GetContentSummary()}");
            }
        }
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to fetch activities.");
            #pragma warning restore CA1848
            Console.WriteLine($"💔 Error: {ex.Message}");
        }
    }
}
