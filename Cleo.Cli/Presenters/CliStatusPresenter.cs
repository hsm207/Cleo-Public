using System.Globalization;
using Cleo.Cli.Aesthetics;
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
        sb.AppendLine(CultureInfo.CurrentCulture, $"{CliAesthetic.SessionStateLabel}: [{model.StateTitle}]");

        // 2. Pull Request 🎁
        sb.AppendLine(CultureInfo.CurrentCulture, $"{CliAesthetic.PullRequestLabel}: {model.PrOutcome}");

        // 3. Last Activity 📝
        var lastActivity = model.LastActivity;
        var timestamp = lastActivity.Timestamp.ToLocalTime().ToString("HH:mm", CultureInfo.CurrentCulture);
        sb.Append(CultureInfo.CurrentCulture, $"{CliAesthetic.LastActivityLabel}: [{timestamp}] {lastActivity.GetContentSummary()}");

        // Polymorphic Thoughts 💭
        var thoughts = lastActivity.GetThoughts().ToList();
        for (var i = 0; i < thoughts.Count; i++)
        {
            var prefix = i == 0 ? $"\n{CliAesthetic.Indent}{CliAesthetic.ThoughtBubble} " : $"\n{CliAesthetic.Indent}   ";
            sb.Append(prefix);
            sb.Append(thoughts[i]);
        }

        // Polymorphic Evidence 📦
        if (lastActivity.Evidence.Count > 0)
        {
            foreach (var artifact in lastActivity.Evidence)
            {
                sb.Append($"\n{CliAesthetic.Indent}{CliAesthetic.ArtifactBox} ");
                sb.Append(artifact.GetSummary());
            }
        }

        return sb.ToString();
    }
}
