using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.InitiateSession;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class NewCommand
{
    private static readonly string[] RepoAliases = { "--repo", "-r" };
    private static readonly string[] BranchAliases = { "--branch", "-b" };
    private static readonly string[] TitleAliases = { "--title", "-t" };

    private readonly InitiateSessionUseCase _useCase;
    private readonly ILogger<NewCommand> _logger;

    public NewCommand(InitiateSessionUseCase useCase, ILogger<NewCommand> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("new", "Assign a specific task to the agent. This initiates the collaboration session.");

        var taskArgument = new Argument<string>("task", "The high-level goal or task for Jules.");
        command.AddArgument(taskArgument);

        var repoOption = new Option<string>(RepoAliases, "The repository name (e.g., sources/github/user/repo)");
        command.AddOption(repoOption);

        var branchOption = new Option<string>(BranchAliases, () => "main", "The starting branch");
        command.AddOption(branchOption);

        var titleOption = new Option<string>(TitleAliases, "A custom title for the session");
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

            Console.WriteLine("✨ Session initiated successfully! 🚀");
            Console.WriteLine($"SessionId: {response.Id}");
            if (response.DashboardUri != null)
            {
                Console.WriteLine($"Portal: {response.DashboardUri}");
            }
        }
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to initiate session.");
            #pragma warning restore CA1848
            Console.WriteLine($"💔 Error: {ex.Message}");
        }
    }
}
