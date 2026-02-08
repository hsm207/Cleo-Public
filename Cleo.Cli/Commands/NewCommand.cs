using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Core.UseCases;
using Cleo.Core.UseCases.InitiateSession;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class NewCommand
{
    private readonly IUseCase<InitiateSessionRequest, InitiateSessionResponse> _useCase;
    private readonly ILogger<NewCommand> _logger;

    private static readonly string[] TaskAliases = { "--task", "-t" };
    private static readonly string[] RepoAliases = { "--repo", "-r" };
    private static readonly string[] BranchAliases = { "--branch", "-b" };
    private static readonly string[] TitleAliases = { "--title", "-n" };

    public NewCommand(IUseCase<InitiateSessionRequest, InitiateSessionResponse> useCase, ILogger<NewCommand> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("new", "Create a new engineering session.");

        var taskOption = new Option<string>(TaskAliases, "The description of the task to perform.") { IsRequired = true };
        var repoOption = new Option<string>(RepoAliases, "The target repository path.") { IsRequired = true };
        var branchOption = new Option<string>(BranchAliases, "The starting branch name.") { IsRequired = true };
        var titleOption = new Option<string>(TitleAliases, "The title for the session.");

        command.AddOption(taskOption);
        command.AddOption(repoOption);
        command.AddOption(branchOption);
        command.AddOption(titleOption);

        command.SetHandler(async (task, repo, branch, title) => await ExecuteAsync(task, repo, branch, title), taskOption, repoOption, branchOption, titleOption);

        return command;
    }

    private async Task ExecuteAsync(string task, string repo, string branch, string? title)
    {
        #pragma warning disable CA1848
        _logger.LogInformation("Initiating session for task: {Task}", task);
        #pragma warning restore CA1848

        try
        {
            var request = new InitiateSessionRequest(task, repo, branch, title);
            var result = await _useCase.ExecuteAsync(request).ConfigureAwait(false);

            Console.WriteLine("✨ Session created successfully! 🚀");
            Console.WriteLine($"🔗 Handle: {result.Id}");
            
            if (result.DashboardUri != null)
            {
                Console.WriteLine($"🌐 Dashboard: {result.DashboardUri}");
            }

            if (result.IsPrAutomated)
            {
                Console.WriteLine("🤖 Auto-PR Protocol: ACTIVE 🔥");
            }

            Console.WriteLine("\nTime to make some magic happen! 🪄✨");
        }
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "Failed to initiate session.");
            #pragma warning restore CA1848
            Console.WriteLine($"Error: {ex.Message}");
        }
        #pragma warning restore CA1031
    }
}
