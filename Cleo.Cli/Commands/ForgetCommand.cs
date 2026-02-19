using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Cli.Services;
using Cleo.Cli.Presenters;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ForgetSession;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class ForgetCommand
{
    private readonly IForgetSessionUseCase _useCase;
    private readonly IStatusPresenter _presenter;
    private readonly IHelpProvider _helpProvider;
    private readonly ILogger<ForgetCommand> _logger;

    public ForgetCommand(
        IForgetSessionUseCase useCase,
        IStatusPresenter presenter,
        IHelpProvider helpProvider,
        ILogger<ForgetCommand> logger)
    {
        _useCase = useCase;
        _presenter = presenter;
        _helpProvider = helpProvider;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("forget", _helpProvider.GetCommandDescription("Forget_Description"));

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
            var request = new ForgetSessionRequest(id);
            await _useCase.ExecuteAsync(request).ConfigureAwait(false);

            _presenter.PresentSuccess(string.Format(System.Globalization.CultureInfo.CurrentCulture, _helpProvider.GetResource("Forget_Success"), sessionId));
        }
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "Failed to forget session.");
            #pragma warning restore CA1848
            _presenter.PresentError(ex.Message);
        }
    }
}
