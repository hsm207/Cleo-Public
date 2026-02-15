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
        // Note: Timestamp is already formatted in the VM
        sb.Append(CultureInfo.CurrentCulture, $"{CliAesthetic.LastActivityLabel}: [{model.LastActivityTime}] {model.LastActivitySummary}");

        // Polymorphic Thoughts 💭
        var thoughts = model.LastActivityThoughts;
        for (var i = 0; i < thoughts.Count; i++)
        {
            var prefix = i == 0 ? $"\n{CliAesthetic.Indent}{CliAesthetic.ThoughtBubble} " : $"\n{CliAesthetic.Indent}   ";
            sb.Append(prefix);
            sb.Append(thoughts[i]);
        }

        // Polymorphic Evidence 📦
        var artifacts = model.LastActivityArtifactSummaries;
        if (artifacts.Count > 0)
        {
            foreach (var summary in artifacts)
            {
                sb.Append($"\n{CliAesthetic.Indent}{CliAesthetic.ArtifactBox} ");
                sb.Append(summary);
            }
        }

        return sb.ToString();
    }
}
