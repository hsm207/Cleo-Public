using System.Globalization;
using Cleo.Cli.Models;

namespace Cleo.Cli.Presenters;

/// <summary>
/// A concrete implementation of the status presenter for the CLI.
/// </summary>
internal sealed class CliStatusPresenter : IStatusPresenter
{
    public string Format(StatusViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var sb = new System.Text.StringBuilder();

        // 1. Session State 🧘‍♀️
        sb.AppendLine(CultureInfo.CurrentCulture, $"🧘‍♀️ Session State: [{model.StateTitle}]");

        // 2. Pull Request 🎁
        sb.AppendLine(CultureInfo.CurrentCulture, $"🎁 Pull Request: {model.PrOutcome}");

        // 3. Last Activity 📝
        var lastActivity = model.LastActivity;
        var timestamp = lastActivity.Timestamp.ToLocalTime().ToString("HH:mm", CultureInfo.CurrentCulture);
        sb.Append(CultureInfo.CurrentCulture, $"📝 Last Activity: [{timestamp}] {lastActivity.GetContentSummary()}");

        // Polymorphic Thoughts 💭
        var thoughts = lastActivity.GetThoughts().ToList();
        for (var i = 0; i < thoughts.Count; i++)
        {
            var prefix = i == 0 ? "\n          💭 " : "\n             ";
            sb.Append(prefix);
            sb.Append(thoughts[i]);
        }

        return sb.ToString();
    }
}
