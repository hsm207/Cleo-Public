using System.Globalization;
using Cleo.Cli.Models;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.RefreshPulse;

namespace Cleo.Cli.Services;

/// <summary>
/// Responsible for interpreting the session state and PR status (The Policy).
/// Fulfills the Single Responsibility Principle (SRP).
/// </summary>
internal sealed class SessionStatusEvaluator : ISessionStatusEvaluator
{
    private readonly IHelpProvider _helpProvider;

    public SessionStatusEvaluator(IHelpProvider helpProvider)
    {
        _helpProvider = helpProvider;
    }

    public StatusViewModel Evaluate(RefreshPulseResponse response)
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
            EvaluatePrOutcome(response.State, response.PullRequest, response.HasUnsubmittedSolution),
            time,
            lastActivity.Headline,
            lastActivity.SubHeadline,
            thoughts.AsReadOnly(),
            artifactSummaries.AsReadOnly());
    }

    private string FormatStateTitle(SessionState state) => state switch
    {
        SessionState.AwaitingPlanApproval or SessionState.AwaitingFeedback => _helpProvider.GetResource("Status_State_Waiting"),
        SessionState.Idle => _helpProvider.GetResource("Status_State_Finished"),
        SessionState.Broken or SessionState.Interrupted => _helpProvider.GetResource("Status_State_Stalled"),
        SessionState.Working or SessionState.Planning => _helpProvider.GetResource("Status_State_Working"),
        _ => state.ToString()
    };

    private string EvaluatePrOutcome(SessionState state, PullRequest? pr, bool hasUnsubmittedSolution)
    {
        if (pr == null)
        {
            if (hasUnsubmittedSolution)
            {
                return _helpProvider.GetResource("Status_PR_SolutionReady");
            }

            return state switch
            {
                SessionState.Working or SessionState.Planning => _helpProvider.GetResource("Status_PR_InProgress"),
                SessionState.AwaitingPlanApproval => _helpProvider.GetResource("Status_PR_AwaitingPlanApproval"),
                SessionState.AwaitingFeedback => _helpProvider.GetResource("Status_PR_AwaitingResponse"),
                SessionState.Idle => _helpProvider.GetResource("Status_PR_FinishedNoPR"),
                SessionState.Broken or SessionState.Interrupted => _helpProvider.GetResource("Status_PR_Stalled"),
                _ => _helpProvider.GetResource("Status_PR_InProgress")
            };
        }

        var prInfo = $"{pr.HeadRef} | {pr.Url}";

        var template = state switch
        {
            SessionState.Working or SessionState.Planning => _helpProvider.GetResource("Status_PR_Iterating"),
            SessionState.AwaitingPlanApproval => _helpProvider.GetResource("Status_PR_AwaitingPlanApprovalWithPR"),
            SessionState.AwaitingFeedback => _helpProvider.GetResource("Status_PR_AwaitingResponseWithPR"),
            SessionState.Idle => _helpProvider.GetResource("Status_PR_SuccessWithPR"),
            SessionState.Broken or SessionState.Interrupted => _helpProvider.GetResource("Status_PR_StalledWithPR"),
            _ => "{0}"
        };

        return string.Format(CultureInfo.CurrentCulture, template, prInfo);
    }
}
