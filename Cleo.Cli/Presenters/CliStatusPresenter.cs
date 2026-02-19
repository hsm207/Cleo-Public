using System.CommandLine;
using System.Globalization;
using Cleo.Cli.Aesthetics;
using Cleo.Cli.Models;
using Cleo.Cli.Services;

namespace Cleo.Cli.Presenters;

/// <summary>
/// A concrete implementation of the status presenter for the CLI.
/// </summary>
internal sealed class CliStatusPresenter : IStatusPresenter
{
    private readonly IConsole _console;
    private readonly IHelpProvider _helpProvider;

    public CliStatusPresenter(IConsole console, IHelpProvider helpProvider)
    {
        _console = console;
        _helpProvider = helpProvider;
    }

    public void PresentSuccess(string message)
    {
        _console.Out.Write(message);
        _console.Out.Write(Environment.NewLine);
    }

    public void PresentNewSession(string sessionId, string? dashboardUri)
    {
        _console.Out.Write(_helpProvider.GetResource("New_Success"));
        _console.Out.Write(Environment.NewLine);
        _console.Out.Write(string.Format(CultureInfo.CurrentCulture, _helpProvider.GetResource("New_SessionId"), sessionId));
        _console.Out.Write(Environment.NewLine);
        if (dashboardUri != null)
        {
            _console.Out.Write(string.Format(CultureInfo.CurrentCulture, _helpProvider.GetResource("New_Portal"), dashboardUri));
            _console.Out.Write(Environment.NewLine);
        }
    }

    public void PresentWarning(string message)
    {
        _console.Out.Write(message);
        _console.Out.Write(Environment.NewLine);
    }

    public void PresentError(string message)
    {
        _console.Out.Write(string.Format(CultureInfo.CurrentCulture, _helpProvider.GetResource("New_Error"), message));
        _console.Out.Write(Environment.NewLine);
    }

    public void PresentStatus(StatusViewModel model)
    {
        _console.Out.Write(Format(model));
    }

    private static string Format(StatusViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var sb = new System.Text.StringBuilder();

        // 1. Session State 🧘‍♀️
        sb.AppendLine(CultureInfo.CurrentCulture, $"{CliAesthetic.SessionStateLabel}: [{model.StateTitle}]");

        // 2. Pull Request 🎁
        sb.AppendLine(CultureInfo.CurrentCulture, $"{CliAesthetic.PullRequestLabel}: {model.PrOutcome}");

        // 3. Last Activity 📝
        sb.Append(CultureInfo.CurrentCulture, $"{CliAesthetic.LastActivityLabel}: [{model.LastActivityTime}] {model.LastActivityHeadline}");

        if (!string.IsNullOrWhiteSpace(model.LastActivitySubHeadline))
        {
            sb.Append(CultureInfo.CurrentCulture, $"\n{CliAesthetic.Indent}{model.LastActivitySubHeadline}");
        }

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
