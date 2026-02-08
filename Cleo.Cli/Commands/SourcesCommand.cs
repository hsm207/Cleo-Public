using System.CommandLine;
using Cleo.Core.UseCases.BrowseSources;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

internal sealed class SourcesCommand
{
    private readonly IBrowseSourcesUseCase _useCase;
    private readonly ILogger<SourcesCommand> _logger;

    public SourcesCommand(IBrowseSourcesUseCase useCase, ILogger<SourcesCommand> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("sources", "List available sources in Jules 🌍");

        command.SetHandler(async () => await ExecuteAsync());

        return command;
    }

    private async Task ExecuteAsync()
    {
        try
        {
            var response = await _useCase.ExecuteAsync(new BrowseSourcesRequest()).ConfigureAwait(false);

            if (response.Sources.Count == 0)
            {
                Console.WriteLine("📭 No sources found. Have you connected your GitHub account to Jules? 💖");
                return;
            }

            Console.WriteLine("📡 Available Sources:");
            foreach (var source in response.Sources)
            {
                Console.WriteLine($"- {source.Name} ({source.Owner}/{source.Repo})");
            }
        }
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to list sources.");
            #pragma warning restore CA1848
            Console.WriteLine($"💔 Something went wrong: {ex.Message}");
        }
        #pragma warning restore CA1031
    }
}
