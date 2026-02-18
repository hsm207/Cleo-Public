using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.RefreshPulse;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class CheckinCommand
{
    private readonly IRefreshPulseUseCase _useCase;
    private readonly IStatusPresenter _presenter;
    private readonly ILogger<CheckinCommand> _logger;

    public CheckinCommand(
        IRefreshPulseUseCase useCase, 
        IStatusPresenter presenter,
        ILogger<CheckinCommand> logger)
    {
        _useCase = useCase;
        _presenter = presenter;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("checkin", "Check in on the progress and state of a session 🧘‍♀️");

        var sessionIdArgument = new Argument<string>("sessionId", "The session ID (e.g., sessions/123).");
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
                Console.WriteLine(response.Warning);
            }

            var viewModel = SessionStatusEvaluator.Evaluate(response);
            Console.Write(_presenter.Format(viewModel));
        }
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to fetch status.");
            #pragma warning restore CA1848
            Console.WriteLine($"💔 Error: {ex.Message}");
        }
    }
}
