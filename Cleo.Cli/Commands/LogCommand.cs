using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.BrowseHistory;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class LogCommand
{
    private readonly IBrowseHistoryUseCase _useCase;
    private readonly ILogger<LogCommand> _logger;

    public LogCommand(IBrowseHistoryUseCase useCase, ILogger<LogCommand> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("log", "Historical audit trail and artifact archaeology 🏺");

        // Subcommand: view (was activities)
        command.AddCommand(BuildViewCommand());

        // Implementing Recursive Signaling 🧠✨
        command.Description += " More specialized subcommands available. Use --help to explore further.";

        return command;
    }

    private Command BuildViewCommand()
    {
        var command = new Command("view", "View the Session Log for a session 📜");

        var sessionIdArgument = new Argument<string>("sessionId", "The session ID.");
        command.AddArgument(sessionIdArgument);

        var allOption = new Option<bool>("--all", "Display all activities, including technical heartbeats.");
        command.AddOption(allOption);

        var limitOption = new Option<int?>("--limit", "Limit the number of activities displayed.");
        command.AddOption(limitOption);

        command.SetHandler(async (sessionId, all, limit) => await ExecuteAsync(sessionId, all, limit), sessionIdArgument, allOption, limitOption);

        return command;
    }

    private async Task ExecuteAsync(string sessionId, bool showAll, int? limit)
    {
        try
        {
            var id = new SessionId(sessionId);
            var request = new BrowseHistoryRequest(id);
            var response = await _useCase.ExecuteAsync(request).ConfigureAwait(false);

            if (response.History.Count == 0)
            {
                Console.WriteLine("📭 No activities found yet. Stay tuned! 📻");
                return;
            }

            Console.WriteLine($"📜 Activities for {sessionId}:");

            if (showAll)
            {
                RenderAllActivities(response.History);
            }
            else
            {
                RenderSignificantActivities(response.History, limit ?? 10);
            }

            if (response.PullRequest != null)
            {
                Console.WriteLine($"\n🎁 Pull Request: {response.PullRequest.Url}");
            }
        }
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to fetch activities.");
            #pragma warning restore CA1848
            Console.WriteLine($"💔 Error: {ex.Message}");
        }
    }

    private static void RenderAllActivities(IReadOnlyList<SessionActivity> history)
    {
        foreach (var activity in history)
        {
            RenderActivity(activity);
        }
        Console.WriteLine($"Showing all {history.Count} activities.");
    }

    private static void RenderSignificantActivities(IReadOnlyList<SessionActivity> history, int limit)
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

        // Truncation Marker
        if (hiddenSignificantCount > 0)
        {
            Console.WriteLine($"... [{hiddenSignificantCount} earlier activities hidden] ...");
        }

        int lastDisplayedIndex = -1;

        // If we truncated significant activities, start tracking gaps from the last hidden significant activity.
        if (hiddenSignificantCount > 0)
        {
            lastDisplayedIndex = significantIndices[startIndex - 1];
        }

        foreach (var currentIndex in displayedIndices)
        {
            // Calculate gap (number of non-significant activities between last displayed and current)
            var gap = currentIndex - lastDisplayedIndex - 1;

            if (gap > 0)
            {
                Console.WriteLine($"... [{gap} heartbeats hidden] ...");
            }

            RenderActivity(history[currentIndex]);
            lastDisplayedIndex = currentIndex;
        }

        var displayedCount = displayedIndices.Count;
        var totalHeartbeatsHidden = history.Count - totalSignificantCount;

        Console.WriteLine($"Showing {displayedCount} of {totalSignificantCount} significant activities ({totalHeartbeatsHidden} total heartbeats hidden). Use --all to see the full history.");
    }

    /// <summary>
    /// Renders a single activity using the UX Goddess Design (RFC 009) 
    /// and Human-Centric Alignment (RFC 013).
    /// </summary>
    private static void RenderActivity(SessionActivity activity)
    {
        var symbol = GetSymbol(activity);
        var summary = activity.GetContentSummary();
        
        // Fallback for empty summaries in progress updates
        if (string.IsNullOrWhiteSpace(summary) && activity is ProgressActivity)
        {
            summary = "Working...";
        }

        // Header line: Symbol + Timestamp + Core Content
        Console.WriteLine($"{symbol} [{activity.Timestamp.ToLocalTime():HH:mm}] {summary}");

        // RFC 013: Multi-line Activity Alignment Policy 📏✨
        // "The 💭 Thought must be indented by exactly 10 spaces to align under the timestamp"
        const string LogIndent = "          "; // 10 spaces

        // Polymorphic Thoughts 💭
        var thoughts = activity.GetThoughts().ToList();
        for (var i = 0; i < thoughts.Count; i++)
        {
            var prefix = i == 0 ? $"{LogIndent}{Cleo.Cli.Aesthetics.CliAesthetic.ThoughtBubble} " : $"{LogIndent}   ";
            Console.WriteLine($"{prefix}{thoughts[i]}");
        }

        // Polymorphic Evidence 📦
        foreach (var artifact in activity.Evidence)
        {
            Console.WriteLine($"{LogIndent}{Cleo.Cli.Aesthetics.CliAesthetic.ArtifactBox} {artifact.GetSummary()}");
        }
    }

    private static string GetSymbol(SessionActivity activity) => activity switch
    {
        MessageActivity m when m.Originator == ActivityOriginator.User => "👤", // User Command
        MessageActivity m when m.Originator == ActivityOriginator.Agent => "👸", // Agent Message
        MessageActivity => "💬", // Fallback for other messages

        PlanningActivity => "🗺️", // Plan Generated

        ProgressActivity p when !string.IsNullOrWhiteSpace(p.Thought) => "🧠", // Agent Thought (Reasoning Signal)
        ProgressActivity => "📡", // Pulse/Heartbeat (Trace Signal)

        ApprovalActivity => "✅", // Approval
        CompletionActivity => "🏁", // Success
        FailureActivity => "💥", // Failure

        _ => "🔹" // Default
    };
}
