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

    private readonly InitiateSessionUseCase _useCase;
    private readonly ILogger<NewCommand> _logger;

    public NewCommand(InitiateSessionUseCase useCase, ILogger<NewCommand> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("new", "Initiate a new engineering session ✨");

        var promptArgument = new Argument<string>("prompt", "The task description or prompt for Jules");
        command.AddArgument(promptArgument);

        var repoOption = new Option<string>(RepoAliases, "The repository name (format: sources/{source})");
        command.AddOption(repoOption);

        var branchOption = new Option<string>(BranchAliases, () => "main", "The starting branch");
        command.AddOption(branchOption);

        command.SetHandler(async (prompt, repo, branch) => await ExecuteAsync(prompt, repo, branch), promptArgument, repoOption, branchOption);

        return command;
    }

    private async Task ExecuteAsync(string prompt, string repo, string branch)
    {
        try
        {
            var request = new InitiateSessionRequest(prompt, repo, branch);
            var response = await _useCase.ExecuteAsync(request).ConfigureAwait(false);

            Console.WriteLine("✨ Session initiated successfully! 🚀");
            Console.WriteLine($"Handle: {response.Id}");
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
