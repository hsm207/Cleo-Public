using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Core.UseCases.ListSessions;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class ListCommand
{
    private readonly IListSessionsUseCase _useCase;
    private readonly ILogger<ListCommand> _logger;

    public ListCommand(IListSessionsUseCase useCase, ILogger<ListCommand> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("list", "List sessions in the local Session Registry 📋");

        command.SetHandler(async () => await ExecuteAsync());

        return command;
    }

    private async Task ExecuteAsync()
    {
        try
        {
            var response = await _useCase.ExecuteAsync(new ListSessionsRequest()).ConfigureAwait(false);

            if (response.Sessions.Count == 0)
            {
                Console.WriteLine("📭 No active sessions found. Time to start something new? 💖");
                return;
            }

            Console.WriteLine("📋 Current Sessions:");
            foreach (var session in response.Sessions)
            {
                Console.WriteLine($"- [{session.Id}] {session.Task} ({session.Pulse.Status})");
            }
        }
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to list sessions.");
            #pragma warning restore CA1848
            Console.WriteLine($"💔 Error: {ex.Message}");
        }
    }
}
