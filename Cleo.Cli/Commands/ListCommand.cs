using System.CommandLine;
using Cleo.Core.UseCases.ListMissions;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

internal sealed class ListCommand
{
    private readonly IListMissionsUseCase _useCase;
    private readonly ILogger<ListCommand> _logger;

    public ListCommand(IListMissionsUseCase useCase, ILogger<ListCommand> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("list", "List all active missions from the global registry 🌍");

        command.SetHandler(async () => await ExecuteAsync());

        return command;
    }

    private async Task ExecuteAsync()
    {
        try
        {
            var response = await _useCase.ExecuteAsync(new ListMissionsRequest()).ConfigureAwait(false);

            if (response.Missions.Count == 0)
            {
                Console.WriteLine("📭 No active missions found. Time to start something new? 💖");
                return;
            }

            Console.WriteLine("📋 Current Missions:");
            foreach (var session in response.Missions)
            {
                Console.WriteLine($"- [{session.Id.Value}] {session.Task} ({session.Pulse.Status})");
            }
        }
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to list sessions.");
            #pragma warning restore CA1848
            Console.WriteLine($"💔 Something went wrong: {ex.Message}");
        }
        #pragma warning restore CA1031
    }
}
