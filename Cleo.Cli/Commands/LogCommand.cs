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
        var command = new Command(_helpProvider.GetResource("Cmd_Log_Name"), _helpProvider.GetCommandDescription("Log_Description"));

        // Subcommand: view (was activities)
        command.AddCommand(BuildViewCommand());

        return command;
    }

    private Command BuildViewCommand()
    {
        var command = new Command(_helpProvider.GetResource("Cmd_View_Name"), _helpProvider.GetCommandDescription("Log_View_Description"));

        var sessionIdArgument = new Argument<string>(_helpProvider.GetResource("Arg_SessionId_Name"), _helpProvider.GetCommandDescription("Log_SessionId"));
        command.AddArgument(sessionIdArgument);

        var allAliases = _helpProvider.GetResource("Opt_All_Aliases").Split(',');
        var allOption = new Option<bool>(allAliases, _helpProvider.GetCommandDescription("Log_All"));
        command.AddOption(allOption);

        var limitAliases = _helpProvider.GetResource("Opt_Limit_Aliases").Split(',');
        var limitOption = new Option<int?>(limitAliases, _helpProvider.GetCommandDescription("Log_Limit"));
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
