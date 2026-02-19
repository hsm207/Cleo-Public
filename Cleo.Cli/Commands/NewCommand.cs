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
    private static readonly string[] RepoAliases = { "--repo", "-r" };
    private static readonly string[] BranchAliases = { "--branch", "-b" };
    private static readonly string[] TitleAliases = { "--title", "-t" };

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
        var command = new Command("new", _helpProvider.GetCommandDescription("New_Description"));

        var taskArgument = new Argument<string>("task", _helpProvider.GetCommandDescription("New_TaskArg_Description"));
        command.AddArgument(taskArgument);

        var repoOption = new Option<string>(RepoAliases, _helpProvider.GetCommandDescription("New_RepoOption_Description"));
        command.AddOption(repoOption);

        var branchOption = new Option<string>(BranchAliases, () => "main", _helpProvider.GetCommandDescription("New_BranchOption_Description"));
        command.AddOption(branchOption);

        var titleOption = new Option<string>(TitleAliases, _helpProvider.GetCommandDescription("New_TitleOption_Description"));
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
