using System.CommandLine;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ApprovePlan;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

internal static class ApproveCommand
{
    public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new Command("approve", "Approve a generated plan 👍");

        var handleArgument = new Argument<string>("handle", "The session handle (ID).");
        var planIdArgument = new Argument<string>("planId", "The ID of the plan to approve.");
        command.AddArgument(handleArgument);
        command.AddArgument(planIdArgument);

        command.SetHandler(async (handle, planId) =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("ApproveCommand");
            var useCase = serviceProvider.GetRequiredService<IApprovePlanUseCase>();

            try
            {
                var sessionId = new SessionId(handle);
                var request = new ApprovePlanRequest(sessionId, planId);
                var response = await useCase.ExecuteAsync(request).ConfigureAwait(false);

                Console.WriteLine($"✅ Plan {response.PlanId} approved for session {handle} at {response.ApprovedAt:t}! Let's go! 🚀");
            }
            #pragma warning disable CA1031
            catch (Exception ex)
            {
                #pragma warning disable CA1848
                logger.LogError(ex, "❌ Failed to approve plan.");
                #pragma warning restore CA1848
                Console.WriteLine($"💔 Something went wrong: {ex.Message}");
            }
            #pragma warning restore CA1031
        }, handleArgument, planIdArgument);

        return command;
    }
}
