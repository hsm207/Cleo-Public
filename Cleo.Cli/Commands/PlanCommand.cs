using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Cli.Services;
using Cleo.Cli.Presenters;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ViewPlan;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class PlanCommand : ICommandGroup
{
    private readonly ApproveCommand _approveCommand;
    private readonly IViewPlanUseCase _viewPlanUseCase;
    private readonly IStatusPresenter _presenter;
    private readonly IHelpProvider _helpProvider;
    private readonly ILogger<PlanCommand> _logger;

    public PlanCommand(
        ApproveCommand approveCommand,
        IViewPlanUseCase viewPlanUseCase,
        IStatusPresenter presenter,
        IHelpProvider helpProvider,
        ILogger<PlanCommand> logger)
    {
        _approveCommand = approveCommand;
        _viewPlanUseCase = viewPlanUseCase;
        _presenter = presenter;
        _helpProvider = helpProvider;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command(_helpProvider.GetResource("Cmd_Plan_Name"), _helpProvider.GetCommandDescription("Plan_Description"));

        command.AddCommand(_approveCommand.Build());
        command.AddCommand(BuildViewCommand());

        return command;
    }

    private Command BuildViewCommand()
    {
        var command = new Command(_helpProvider.GetResource("Cmd_View_Name"), _helpProvider.GetCommandDescription("Plan_View_Description"));

        var sessionIdArgument = new Argument<string>(_helpProvider.GetResource("Arg_SessionId_Name"), _helpProvider.GetCommandDescription("Plan_SessionId"));
        command.AddArgument(sessionIdArgument);

        command.SetHandler(async (sessionId) => await ExecuteViewAsync(sessionId), sessionIdArgument);

        return command;
    }

    private async Task ExecuteViewAsync(string sessionId)
    {
        try
        {
            var id = new SessionId(sessionId);
            var request = new ViewPlanRequest(id);
            var response = await _viewPlanUseCase.ExecuteAsync(request).ConfigureAwait(false);

            if (!response.HasPlan)
            {
                _presenter.PresentEmptyPlan();
                return;
            }

            _presenter.PresentPlan(response);
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to view plan.");
#pragma warning restore CA1848
            _presenter.PresentError(ex.Message);
        }
    }
}
