using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Cli.Services;
using Cleo.Cli.Presenters;
using Cleo.Core.UseCases.BrowseSources;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class ReposCommand
{
    private readonly IBrowseSourcesUseCase _useCase;
    private readonly IStatusPresenter _presenter;
    private readonly IHelpProvider _helpProvider;
    private readonly ILogger<ReposCommand> _logger;

    public ReposCommand(
        IBrowseSourcesUseCase useCase,
        IStatusPresenter presenter,
        IHelpProvider helpProvider,
        ILogger<ReposCommand> logger)
    {
        _useCase = useCase;
        _presenter = presenter;
        _helpProvider = helpProvider;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command(_helpProvider.GetResource("Cmd_Repos_Name"), _helpProvider.GetCommandDescription("Repos_Description"));

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
                _presenter.PresentEmptyRepositories();
                return;
            }

            var repoList = new List<string>();
            foreach (var source in response.Sources)
            {
                repoList.Add(source.Name);
            }

            _presenter.PresentRepositories(repoList);
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to fetch repositories.");
#pragma warning restore CA1848
            _presenter.PresentError(ex.Message);
        }
    }
}
