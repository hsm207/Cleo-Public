using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Cli.Services;
using Cleo.Cli.Presenters;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.BrowseHistory;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class LogCommand : ICommandGroup
{
    private readonly IBrowseHistoryUseCase _useCase;
    private readonly IStatusPresenter _presenter;
    private readonly IHelpProvider _helpProvider;
    private readonly ILogger<LogCommand> _logger;

    public LogCommand(IBrowseHistoryUseCase useCase, IStatusPresenter presenter, IHelpProvider helpProvider, ILogger<LogCommand> logger)
    {
        _useCase = useCase;
        _presenter = presenter;
        _helpProvider = helpProvider;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("log", _helpProvider.GetCommandDescription("Log_Description"));

        // Subcommand: view (was activities)
        command.AddCommand(BuildViewCommand());

        command.Description += " More specialized subcommands available. Use --help to explore further.";

        return command;
    }

    private Command BuildViewCommand()
    {
        var command = new Command("view", "View the Session Log for a session 📜");

        var sessionIdArgument = new Argument<string>("sessionId", "The session ID (e.g., sessions/123).");
        command.AddArgument(sessionIdArgument);

        var allOption = new Option<bool>("--all", "Display all activities, including technical heartbeats.");
        command.AddOption(allOption);

        var limitOption = new Option<int?>("--limit", "Limit the number of activities displayed.");
        command.AddOption(limitOption);

        command.SetHandler(async (sessionId, all, limit) => await ExecuteAsync(sessionId, all, limit), sessionIdArgument, allOption, limitOption);

        return command;
    }

    private async Task ExecuteAsync(string sessionId, bool showAll, int? limit)
    {
        try
        {
            var id = new SessionId(sessionId);
            var request = new BrowseHistoryRequest(id);
            var response = await _useCase.ExecuteAsync(request).ConfigureAwait(false);

            if (response.History.Count == 0)
            {
                _presenter.PresentEmptyLog();
                return;
            }

            _presenter.PresentActivityLog(sessionId, response.History, showAll, limit, response.PullRequest);
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to fetch activities.");
#pragma warning restore CA1848
            _presenter.PresentError(ex.Message);
        }
    }
}
