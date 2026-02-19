using System.CommandLine;
using System.Globalization;
using Cleo.Cli.Aesthetics;
using Cleo.Cli.Models;
using Cleo.Cli.Services;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Cli.Presenters;

/// <summary>
/// A concrete implementation of the status presenter for the CLI.
/// </summary>
internal sealed class CliStatusPresenter : IStatusPresenter
{
    private static readonly string[] _newlines = ["\r\n", "\r", "\n"];
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

    public void PresentEmptyPlan()
    {
        _console.Out.Write(_helpProvider.GetResource("Plan_Empty"));
        _console.Out.Write(Environment.NewLine);
    }

    public void PresentPlan(Cleo.Core.UseCases.ViewPlan.ViewPlanResponse response)
    {
        var planTitle = response.IsApproved ? _helpProvider.GetResource("Plan_Title_Approved") : _helpProvider.GetResource("Plan_Title_Proposed");

        _console.Out.Write(string.Format(CultureInfo.CurrentCulture, _helpProvider.GetResource("Plan_Header"), planTitle, response.PlanId));
        _console.Out.Write(Environment.NewLine);

        _console.Out.Write(string.Format(CultureInfo.CurrentCulture, _helpProvider.GetResource("Plan_Generated"), response.Timestamp));
        _console.Out.Write(Environment.NewLine);

        foreach (var step in response.Steps)
        {
            _console.Out.Write($"{step.Index}. {step.Title}");
            _console.Out.Write(Environment.NewLine);

            if (!string.IsNullOrWhiteSpace(step.Description))
            {
                var lines = step.Description.Split(_newlines, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    _console.Out.Write($"   {line}");
                    _console.Out.Write(Environment.NewLine);
                }
            }
        }
    }

    public void PresentEmptyRepositories()
    {
        _console.Out.Write(_helpProvider.GetResource("Repos_Empty"));
        _console.Out.Write(Environment.NewLine);
    }

    public void PresentRepositories(IEnumerable<string> repositories)
    {
        _console.Out.Write(_helpProvider.GetResource("Repos_Header"));
        _console.Out.Write(Environment.NewLine);
        foreach (var repo in repositories)
        {
            _console.Out.Write(string.Format(CultureInfo.CurrentCulture, _helpProvider.GetResource("Repos_Item_Format"), repo));
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

    public void PresentSessionList(IEnumerable<(string Id, string Task, string State)> sessions)
    {
        _console.Out.Write(_helpProvider.GetResource("List_Header"));
        _console.Out.Write(Environment.NewLine);
        foreach (var (id, task, state) in sessions)
        {
            _console.Out.Write(string.Format(CultureInfo.CurrentCulture, _helpProvider.GetResource("List_Item_Format"), id, task, state));
            _console.Out.Write(Environment.NewLine);
        }
    }

    public void PresentEmptyList()
    {
        _console.Out.Write(_helpProvider.GetResource("List_Empty"));
        _console.Out.Write(Environment.NewLine);
    }

    public void PresentEmptyLog()
    {
        _console.Out.Write(_helpProvider.GetResource("Log_Empty"));
        _console.Out.Write(Environment.NewLine);
    }

    public void PresentActivityLog(string sessionId, IEnumerable<SessionActivity> activities, bool showAll, int? limit, PullRequest? pullRequest)
    {
        var history = activities.ToList();
        _console.Out.Write(string.Format(CultureInfo.CurrentCulture, _helpProvider.GetResource("Log_Header"), sessionId));
        _console.Out.Write(Environment.NewLine);

        if (showAll)
        {
            RenderAllActivities(history);
        }
        else
        {
            RenderSignificantActivities(history, limit ?? 10);
        }

        if (pullRequest != null)
        {
            _console.Out.Write(Environment.NewLine);
            _console.Out.Write(string.Format(CultureInfo.CurrentCulture, _helpProvider.GetResource("Log_PullRequest"), pullRequest.Url));
            _console.Out.Write(Environment.NewLine);
        }
    }

    private void RenderAllActivities(List<SessionActivity> history)
    {
        foreach (var activity in history)
        {
            RenderActivity(activity);
        }
        _console.Out.Write(string.Format(CultureInfo.CurrentCulture, _helpProvider.GetResource("Log_ShowingAll"), history.Count));
        _console.Out.Write(Environment.NewLine);
    }

    private void RenderSignificantActivities(List<SessionActivity> history, int limit)
    {
        var significantIndices = new List<int>();
        for (int i = 0; i < history.Count; i++)
        {
            if (history[i].IsSignificant)
            {
                significantIndices.Add(i);
            }
        }

        var totalSignificantCount = significantIndices.Count;
        var startIndex = Math.Max(0, totalSignificantCount - limit);
        var displayedIndices = significantIndices.Skip(startIndex).ToList();
        var hiddenSignificantCount = startIndex;

        if (hiddenSignificantCount > 0)
        {
            _console.Out.Write(string.Format(CultureInfo.CurrentCulture, _helpProvider.GetResource("Log_HiddenEarlier"), hiddenSignificantCount));
            _console.Out.Write(Environment.NewLine);
        }

        int lastDisplayedIndex = -1;

        if (hiddenSignificantCount > 0)
        {
            lastDisplayedIndex = significantIndices[startIndex - 1];
        }

        foreach (var currentIndex in displayedIndices)
        {
            var gap = currentIndex - lastDisplayedIndex - 1;

            if (gap > 0)
            {
                _console.Out.Write(string.Format(CultureInfo.CurrentCulture, _helpProvider.GetResource("Log_HiddenHeartbeats"), gap));
                _console.Out.Write(Environment.NewLine);
            }

            RenderActivity(history[currentIndex]);
            lastDisplayedIndex = currentIndex;
        }

        var displayedCount = displayedIndices.Count;
        var totalHeartbeatsHidden = history.Count - totalSignificantCount;

        _console.Out.Write(string.Format(CultureInfo.CurrentCulture, _helpProvider.GetResource("Log_ShowingSignificant"), displayedCount, totalSignificantCount, totalHeartbeatsHidden));
        _console.Out.Write(Environment.NewLine);
    }

    private void RenderActivity(SessionActivity activity)
    {
        var symbol = activity.GetSymbol();
        var summary = activity.Headline;

        if (string.IsNullOrWhiteSpace(summary) && activity is ProgressActivity)
        {
            summary = _helpProvider.GetResource("Log_DefaultSummary");
        }

        _console.Out.Write($"{symbol} [{activity.Timestamp.ToLocalTime():HH:mm}] {summary}");
        _console.Out.Write(Environment.NewLine);

        const string LogIndent = "          ";

        if (!string.IsNullOrWhiteSpace(activity.SubHeadline))
        {
            _console.Out.Write($"{LogIndent}{activity.SubHeadline}");
            _console.Out.Write(Environment.NewLine);
        }

        var thoughts = activity.GetThoughts().ToList();
        for (var i = 0; i < thoughts.Count; i++)
        {
            var prefix = i == 0 ? $"{LogIndent}{CliAesthetic.ThoughtBubble} " : $"{LogIndent}   ";
            _console.Out.Write($"{prefix}{thoughts[i]}");
            _console.Out.Write(Environment.NewLine);
        }

        foreach (var artifact in activity.Evidence)
        {
            _console.Out.Write($"{LogIndent}{CliAesthetic.ArtifactBox} {artifact.GetSummary()}");
            _console.Out.Write(Environment.NewLine);
        }
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
