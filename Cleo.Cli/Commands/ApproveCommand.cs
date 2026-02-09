using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ApprovePlan;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class ApproveCommand
{
    private readonly IApprovePlanUseCase _useCase;
    private readonly ILogger<ApproveCommand> _logger;

    public ApproveCommand(IApprovePlanUseCase useCase, ILogger<ApproveCommand> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("approve", "Approve a generated plan 👍");

        var sessionIdArgument = new Argument<string>("sessionId", "The session ID.");
        command.AddArgument(sessionIdArgument);

        var planIdArgument = new Argument<string>("planId", "The ID of the plan to approve.");
        command.AddArgument(planIdArgument);

        command.SetHandler(async (sessionId, planId) => await ExecuteAsync(sessionId, planId), sessionIdArgument, planIdArgument);

        return command;
    }

    private async Task ExecuteAsync(string sessionId, string planId)
    {
        try
        {
            var request = new ApprovePlanRequest(new SessionId(sessionId), planId);
            var response = await _useCase.ExecuteAsync(request).ConfigureAwait(false);

            Console.WriteLine($"✅ Plan {response.PlanId} approved for session {sessionId} at {response.ApprovedAt:t}! Let's go! 🚀");
        }
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to approve plan.");
            #pragma warning restore CA1848
            Console.WriteLine($"💔 Error: {ex.Message}");
        }
    }
}
