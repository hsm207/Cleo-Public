using System.CommandLine;
using Cleo.Core.UseCases;
using Cleo.Core.UseCases.InitiateSession;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

internal static class NewCommand
{
    private static readonly string[] TaskAliases = { "--task", "-t" };
    private static readonly string[] RepoAliases = { "--repo", "-r" };
    private static readonly string[] BranchAliases = { "--branch", "-b" };
    private static readonly string[] TitleAliases = { "--title", "-n" };

    public static Command Create(IServiceProvider serviceProvider)
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

        command.SetHandler(async (task, repo, branch, title) =>
        {
            var useCase = serviceProvider.GetRequiredService<IUseCase<InitiateSessionRequest, InitiateSessionResponse>>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("NewCommand");

            #pragma warning disable CA1848
            logger.LogInformation("Initiating session for task: {Task}", task);
            #pragma warning restore CA1848

            try
            {
                var request = new InitiateSessionRequest(task, repo, branch, title);
                var result = await useCase.ExecuteAsync(request).ConfigureAwait(false);

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
                logger.LogError(ex, "Failed to initiate session.");
                #pragma warning restore CA1848
                Console.WriteLine($"Error: {ex.Message}");
            }
            #pragma warning restore CA1031
        }, taskOption, repoOption, branchOption, titleOption);

        return command;
    }
}
