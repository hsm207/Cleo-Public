using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.BrowseHistory;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class LogCommand
{
    private readonly IBrowseHistoryUseCase _useCase;
    private readonly ILogger<LogCommand> _logger;

    public LogCommand(IBrowseHistoryUseCase useCase, ILogger<LogCommand> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("log", "Historical audit trail and artifact archaeology 🏺");

        // Subcommand: view (was activities)
        command.AddCommand(BuildViewCommand());

        return command;
    }

    private Command BuildViewCommand()
    {
        var command = new Command("view", "View the Session Log for a session 📜");

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
            var request = new BrowseHistoryRequest(id);
            var response = await _useCase.ExecuteAsync(request).ConfigureAwait(false);

            if (response.History.Count == 0)
            {
                Console.WriteLine("📭 No activities found yet. Stay tuned! 📻");
                return;
            }

            Console.WriteLine($"📜 Activities for {sessionId}:");
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
