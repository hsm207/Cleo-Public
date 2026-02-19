using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Cli.Services;
using Cleo.Cli.Presenters;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ListSessions;
using Cleo.Core.UseCases.RefreshPulse;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class ListCommand
{
    private readonly IListSessionsUseCase _useCase;
    private readonly IStatusPresenter _presenter;
    private readonly IHelpProvider _helpProvider;
    private readonly ILogger<ListCommand> _logger;

    public ListCommand(IListSessionsUseCase useCase, IStatusPresenter presenter, IHelpProvider helpProvider, ILogger<ListCommand> logger)
    {
        _useCase = useCase;
        _presenter = presenter;
        _helpProvider = helpProvider;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("list", _helpProvider.GetCommandDescription("List_Description"));

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
                _presenter.PresentEmptyList();
                return;
            }

            var sessionList = new List<(string Id, string Task, string State)>();
            foreach (var session in response.Sessions)
            {
                var statusResponse = new RefreshPulseResponse(
                    session.Id,
                    session.Pulse,
                    session.State,
                    session.LastActivity,
                    session.PullRequest);

                var vm = SessionStatusEvaluator.Evaluate(statusResponse);
                sessionList.Add((session.Id.ToString(), session.Task.ToString(), vm.StateTitle));
            }

            _presenter.PresentSessionList(sessionList);
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to list sessions.");
#pragma warning restore CA1848
            _presenter.PresentError(ex.Message);
        }
    }
}
