using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Cli.Services;
using Cleo.Cli.Presenters;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.RefreshPulse;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class CheckinCommand
{
    private readonly IRefreshPulseUseCase _useCase;
    private readonly IStatusPresenter _presenter;
    private readonly IHelpProvider _helpProvider;
    private readonly ILogger<CheckinCommand> _logger;

    public CheckinCommand(
        IRefreshPulseUseCase useCase,
        IStatusPresenter presenter,
        IHelpProvider helpProvider,
        ILogger<CheckinCommand> logger)
    {
        _useCase = useCase;
        _presenter = presenter;
        _helpProvider = helpProvider;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("checkin", _helpProvider.GetCommandDescription("Checkin_Description"));

        var sessionIdArgument = new Argument<string>("sessionId", _helpProvider.GetCommandDescription("Checkin_SessionId"));
        command.AddArgument(sessionIdArgument);

        command.SetHandler(async (sessionId) => await ExecuteAsync(sessionId), sessionIdArgument);

        return command;
    }

    private async Task ExecuteAsync(string sessionId)
    {
        try
        {
            var id = new SessionId(sessionId);
            var request = new RefreshPulseRequest(id);
            var response = await _useCase.ExecuteAsync(request).ConfigureAwait(false);

            if (response.Warning != null)
            {
                _presenter.PresentWarning(response.Warning);
            }

            var viewModel = SessionStatusEvaluator.Evaluate(response);
            _presenter.PresentStatus(viewModel);
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to fetch status.");
#pragma warning restore CA1848
            _presenter.PresentError(ex.Message);
        }
    }
}
