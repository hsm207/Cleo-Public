using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ViewPlan;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class PlanCommand
{
    private static readonly string[] _newlines = ["\r\n", "\r", "\n"];

    private readonly ApproveCommand _approveCommand;
    private readonly IViewPlanUseCase _viewPlanUseCase;
    private readonly ILogger<PlanCommand> _logger;

    public PlanCommand(
        ApproveCommand approveCommand,
        IViewPlanUseCase viewPlanUseCase,
        ILogger<PlanCommand> logger)
    {
        _approveCommand = approveCommand;
        _viewPlanUseCase = viewPlanUseCase;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("plan", "Authoritative roadmap visibility and gating 🗺️");

        command.AddCommand(_approveCommand.Build());
        command.AddCommand(BuildViewCommand());

        return command;
    }

    private Command BuildViewCommand()
    {
        var command = new Command("view", "View the authoritative roadmap for a session 🔭");

        var sessionIdArgument = new Argument<string>("sessionId", "The session ID.");
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
                Console.WriteLine("📭 No approved plan found for this session.");
                return;
            }

            var planTitle = response.IsApproved ? "Approved Plan" : "Proposed Plan";
            Console.WriteLine($"🗺️ {planTitle}: {response.PlanId}");
            Console.WriteLine($"🕒 Generated: {response.Timestamp:g}");
            foreach (var step in response.Steps)
            {
                Console.WriteLine($"{step.Index}. {step.Title}");
                if (!string.IsNullOrWhiteSpace(step.Description))
                {
                    var lines = step.Description.Split(_newlines, StringSplitOptions.None);
                    foreach (var line in lines)
                    {
                        Console.WriteLine($"   {line}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to view plan.");
            #pragma warning restore CA1848
            Console.WriteLine($"💔 Error: {ex.Message}");
        }
    }
}
