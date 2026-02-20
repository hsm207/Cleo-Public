using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Cli.Services;
using Cleo.Cli.Presenters;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ApprovePlan;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class ApproveCommand
{
    private readonly IApprovePlanUseCase _useCase;
    private readonly IStatusPresenter _presenter;
    private readonly IHelpProvider _helpProvider;
    private readonly ILogger<ApproveCommand> _logger;

    public ApproveCommand(IApprovePlanUseCase useCase, IStatusPresenter presenter, IHelpProvider helpProvider, ILogger<ApproveCommand> logger)
    {
        _useCase = useCase;
        _presenter = presenter;
        _helpProvider = helpProvider;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("approve", _helpProvider.GetCommandDescription("Approve_Description"));

        var sessionIdArgument = new Argument<string>("sessionId", _helpProvider.GetCommandDescription("Approve_SessionId"));
        command.AddArgument(sessionIdArgument);

        var planIdArgument = new Argument<string>("planId", _helpProvider.GetCommandDescription("Approve_PlanId"));
        command.AddArgument(planIdArgument);

        command.SetHandler(async (sessionId, planId) => await ExecuteAsync(sessionId, planId), sessionIdArgument, planIdArgument);

        return command;
    }

    private async Task ExecuteAsync(string sessionId, string planId)
    {
        try
        {
            var request = new ApprovePlanRequest(new SessionId(sessionId), new PlanId(planId));
            var response = await _useCase.ExecuteAsync(request).ConfigureAwait(false);

            _presenter.PresentSuccess(string.Format(System.Globalization.CultureInfo.CurrentCulture, _helpProvider.GetResource("Approve_Success"), response.PlanId, sessionId, response.ApprovedAt));
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to approve plan.");
#pragma warning restore CA1848
            _presenter.PresentError(ex.Message);
        }
    }
}
