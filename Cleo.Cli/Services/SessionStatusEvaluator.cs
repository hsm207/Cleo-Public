using System.Globalization;
using Cleo.Cli.Models;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.RefreshPulse;

namespace Cleo.Cli.Services;

/// <summary>
/// Responsible for interpreting the session state and PR status (The Policy).
/// Fulfills the Single Responsibility Principle (SRP).
/// </summary>
internal sealed class SessionStatusEvaluator
{
    public static StatusViewModel Evaluate(RefreshPulseResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var lastActivity = response.LastActivity;

        // Polymorphic extraction for the view model
        var thoughts = lastActivity.GetThoughts().ToList();
        var artifactSummaries = lastActivity.Evidence.Select(e => e.GetSummary()).ToList();

        // Format timestamp for display
        var time = lastActivity.Timestamp.ToLocalTime().ToString("HH:mm", CultureInfo.CurrentCulture);

        return new StatusViewModel(
            FormatStateTitle(response.State),
            EvaluatePrOutcome(response.State, response.PullRequest),
            time,
            lastActivity.GetContentSummary(),
            thoughts.AsReadOnly(),
            artifactSummaries.AsReadOnly());
    }

    private static string FormatStateTitle(SessionState state) => state switch
    {
        SessionState.AwaitingPlanApproval or SessionState.AwaitingFeedback => "Waiting for You",
        SessionState.Idle => "Finished",
        SessionState.Broken or SessionState.Interrupted => "Stalled",
        SessionState.Working or SessionState.Planning => "Working",
        _ => state.ToString()
    };

    private static string EvaluatePrOutcome(SessionState state, PullRequest? pr)
    {
        if (pr == null)
        {
            return state switch
            {
                SessionState.Working or SessionState.Planning => "⏳ In Progress",
                SessionState.AwaitingPlanApproval => "⏳ Awaiting Plan Approval",
                SessionState.AwaitingFeedback => "⏳ Awaiting your response...",
                SessionState.Idle => "WTF?! 🤪 (Finished with no PR)",
                SessionState.Broken or SessionState.Interrupted => "🛑 Stalled",
                _ => "⏳ In Progress"
            };
        }

        return state switch
        {
            SessionState.Working or SessionState.Planning => $"🔄 Iterating | {pr.Url}",
            SessionState.AwaitingPlanApproval => $"⏳ Awaiting Plan Approval | {pr.Url}",
            SessionState.AwaitingFeedback => $"⏳ Awaiting your response... | {pr.Url}",
            SessionState.Idle => $"✅ {pr.Url}",
            SessionState.Broken or SessionState.Interrupted => $"🛑 Stalled | {pr.Url}",
            _ => $"{pr.Url}"
        };
    }
}
