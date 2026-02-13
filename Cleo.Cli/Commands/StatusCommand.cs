using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.RefreshPulse;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class StatusCommand
{
    private readonly IRefreshPulseUseCase _useCase;
    private readonly SessionStatusEvaluator _evaluator;
    private readonly IStatusPresenter _presenter;
    private readonly ILogger<StatusCommand> _logger;

    public StatusCommand(
        IRefreshPulseUseCase useCase, 
        SessionStatusEvaluator evaluator,
        IStatusPresenter presenter,
        ILogger<StatusCommand> logger)
    {
        _useCase = useCase;
        _evaluator = evaluator;
        _presenter = presenter;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("status", "Check the Pulse and SessionState of a session 💓");

        var sessionIdArgument = new Argument<string>("sessionId", "The session ID.");
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

            var viewModel = _evaluator.Evaluate(response);
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
