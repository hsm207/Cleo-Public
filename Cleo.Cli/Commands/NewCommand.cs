using System.CommandLine;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

internal static class NewCommand
{
    private static readonly string[] TaskAliases = { "--task", "-t" };
    private static readonly string[] RepoAliases = { "--repo", "-r" };
    private static readonly string[] BranchAliases = { "--branch", "-b" };

    public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new Command("new", "Create a new engineering session 💎");

        var taskOption = new Option<string>(TaskAliases, "The description of the task to perform.") { IsRequired = true };
        var repoOption = new Option<string>(RepoAliases, "The target repository path.") { IsRequired = true };
        var branchOption = new Option<string>(BranchAliases, "The starting branch name.") { IsRequired = true };

        command.AddOption(taskOption);
        command.AddOption(repoOption);
        command.AddOption(branchOption);

        command.SetHandler(async (task, repo, branch) =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("NewCommand");
            var julesClient = serviceProvider.GetRequiredService<IJulesSessionClient>();
            var writer = serviceProvider.GetRequiredService<ISessionWriter>();

            #pragma warning disable CA1848
            logger.LogInformation("💖 Creating new session for task: {Task}", task);
            #pragma warning restore CA1848

            try
            {
                var taskDescription = new TaskDescription(task);
                var sourceContext = new SourceContext(repo, branch);

                var session = await julesClient.CreateSessionAsync(taskDescription, sourceContext).ConfigureAwait(false);
                await writer.SaveAsync(session).ConfigureAwait(false);

                Console.WriteLine($"✅ Session created successfully, babe! 🚀");
                Console.WriteLine($"🔗 Handle: {session.Id.Value}");
            }
            #pragma warning disable CA1031
            catch (Exception ex)
            {
                #pragma warning disable CA1848
                logger.LogError(ex, "❌ Failed to create session.");
                #pragma warning restore CA1848
                Console.WriteLine($"💔 Something went wrong: {ex.Message}");
            }
            #pragma warning restore CA1031
        }, taskOption, repoOption, branchOption);

        return command;
    }
}
