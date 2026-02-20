using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.UseCases.InitiateSession;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class NewCommand
{
    private readonly IInitiateSessionUseCase _useCase;
    private readonly IStatusPresenter _presenter;
    private readonly IHelpProvider _helpProvider;
    private readonly ILogger<NewCommand> _logger;

    public NewCommand(IInitiateSessionUseCase useCase, IStatusPresenter presenter, IHelpProvider helpProvider, ILogger<NewCommand> logger)
    {
        _useCase = useCase;
        _presenter = presenter;
        _helpProvider = helpProvider;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command(_helpProvider.GetResource("Cmd_New_Name"), _helpProvider.GetCommandDescription("New_Description"));

        var taskArgument = new Argument<string>(_helpProvider.GetResource("Arg_Task_Name"), _helpProvider.GetCommandDescription("New_TaskArg_Description"));
        command.AddArgument(taskArgument);

        var repoAliases = _helpProvider.GetResource("Opt_Repo_Aliases").Split(',');
        var repoOption = new Option<string>(repoAliases, _helpProvider.GetCommandDescription("New_RepoOption_Description"));
        command.AddOption(repoOption);

        var branchAliases = _helpProvider.GetResource("Opt_Branch_Aliases").Split(',');
        var branchOption = new Option<string>(branchAliases, () => _helpProvider.GetResource("Opt_Branch_Default"), _helpProvider.GetCommandDescription("New_BranchOption_Description"));
        command.AddOption(branchOption);

        var titleAliases = _helpProvider.GetResource("Opt_Title_Aliases").Split(',');
        var titleOption = new Option<string>(titleAliases, _helpProvider.GetCommandDescription("New_TitleOption_Description"));
        command.AddOption(titleOption);

        command.SetHandler(async (task, repo, branch, title) => await ExecuteAsync(task, repo, branch, title), taskArgument, repoOption, branchOption, titleOption);

        return command;
    }

    private async Task ExecuteAsync(string task, string repo, string branch, string? title)
    {
        try
        {
            var request = new InitiateSessionRequest(task, repo, branch, title);
            var response = await _useCase.ExecuteAsync(request).ConfigureAwait(false);

            _presenter.PresentNewSession(response.Id.ToString(), response.DashboardUri?.ToString());
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to initiate session.");
#pragma warning restore CA1848
            _presenter.PresentError(ex.Message);
        }
    }
}
