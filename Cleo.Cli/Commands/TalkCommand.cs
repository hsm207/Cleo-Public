using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Cli.Services;
using Cleo.Cli.Presenters;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.Correspond;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class TalkCommand : ICommandGroup
{
    private readonly ICorrespondUseCase _useCase;
    private readonly IStatusPresenter _presenter;
    private readonly IHelpProvider _helpProvider;
    private readonly ILogger<TalkCommand> _logger;

    public TalkCommand(
        ICorrespondUseCase useCase,
        IStatusPresenter presenter,
        IHelpProvider helpProvider,
        ILogger<TalkCommand> logger)
    {
        _useCase = useCase;
        _presenter = presenter;
        _helpProvider = helpProvider;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command(_helpProvider.GetResource("Cmd_Talk_Name"), _helpProvider.GetCommandDescription("Talk_Description"));

        var sessionIdArgument = new Argument<string>(_helpProvider.GetResource("Arg_SessionId_Name"), _helpProvider.GetCommandDescription("Talk_SessionId"));
        command.AddArgument(sessionIdArgument);

        var messageAliases = _helpProvider.GetResource("Opt_Message_Aliases").Split(',');
        var messageOption = new Option<string>(messageAliases, _helpProvider.GetCommandDescription("Talk_Message"))
        {
            IsRequired = true
        };
        command.AddOption(messageOption);

        command.SetHandler(async (sessionId, message) => await ExecuteAsync(sessionId, message), sessionIdArgument, messageOption);

        return command;
    }

    private async Task ExecuteAsync(string sessionId, string message)
    {
        try
        {
            var request = new CorrespondRequest(new SessionId(sessionId), message);
            await _useCase.ExecuteAsync(request).ConfigureAwait(false);

            _presenter.PresentMessageSent();
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to send message.");
#pragma warning restore CA1848
            _presenter.PresentError(ex.Message);
        }
    }
}
