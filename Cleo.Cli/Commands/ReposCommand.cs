using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Core.UseCases.BrowseSources;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class ReposCommand
{
    private readonly IBrowseSourcesUseCase _useCase;
    private readonly ILogger<ReposCommand> _logger;

    public ReposCommand(IBrowseSourcesUseCase useCase, ILogger<ReposCommand> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("repos", "List available GitHub repositories for collaboration 🛰️");

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
                Console.WriteLine("📭 No sources found. Ensure your GitHub account is connected to Jules! 💖");
                return;
            }

            Console.WriteLine("🛰️ Available Repositories:");
            foreach (var source in response.Sources)
            {
                Console.WriteLine($"- {source.Name}");
            }
        }
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to fetch repositories.");
            #pragma warning restore CA1848
            Console.WriteLine($"💔 Error: {ex.Message}");
        }
    }
}
