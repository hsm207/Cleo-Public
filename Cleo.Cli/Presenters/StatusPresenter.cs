using System.Globalization;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.RefreshPulse;

namespace Cleo.Cli.Presenters;

/// <summary>
/// A Humble Object responsible for formatting the status output.
/// Implements the MECE Matrix from RFC 013.
/// </summary>
internal sealed class StatusPresenter
{
    public static string Format(RefreshPulseResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var sb = new System.Text.StringBuilder();

        // 1. Session State 🧘‍♀️
        sb.AppendLine(CultureInfo.CurrentCulture, $"🧘‍♀️ Session State: [{FormatState(response.State)}]");

        // 2. Pull Request 🎁
        sb.AppendLine(CultureInfo.CurrentCulture, $"🎁 Pull Request: {FormatPullRequest(response.State, response.PullRequest)}");

        // 3. Last Activity 📝
        var lastActivity = response.LastActivity;
        var timestamp = lastActivity.Timestamp.ToLocalTime().ToString("HH:mm", CultureInfo.CurrentCulture);
        sb.Append(CultureInfo.CurrentCulture, $"📝 Last Activity: [{timestamp}] {lastActivity.GetContentSummary()}");

        if (lastActivity is ProgressActivity { Thought: not null } progress)
        {
            var lines = progress.Thought.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                var prefix = i == 0 ? "\n          💭 " : "\n             ";
                sb.Append(prefix + lines[i]);
            }
        }

        return sb.ToString();
    }

    private static string FormatState(SessionState state) => state switch
    {
        SessionState.AwaitingPlanApproval => "Waiting for You", // Special case for this state
        SessionState.Idle => "Finished",
        _ => state.ToString()
    };

    private static string FormatPullRequest(SessionState state, PullRequest? pr)
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
