using Cleo.Core.Domain.Common;
using Cleo.Core.Domain.Events;
using Cleo.Core.Domain.Policies;
using Cleo.Core.Domain.Services;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Entities;

/// <summary>
/// The central authority for an autonomous coding collaboration.
/// </summary>
#pragma warning disable CA1062 // Validate arguments of public methods (VIP Lounge: We trust Value Objects)
public class Session : AggregateRoot
{
    private readonly List<SessionActivity> _sessionLog;

    public SessionId Id { get; }
    public string RemoteId { get; }
    public string? Title { get; }
    public TaskDescription Task { get; }
    public SourceContext Source { get; }
    public SessionPulse Pulse { get; private set; }
    public ChangeSet? Solution { get; private set; }
    public PullRequest? PullRequest { get; private set; }
    public Uri? DashboardUri { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? UpdatedAt { get; }
    public bool? RequiresPlanApproval { get; }
    public AutomationMode Mode { get; }

    public IReadOnlyCollection<SessionActivity> SessionLog => _sessionLog.AsReadOnly();

    public IReadOnlyCollection<SessionActivity> GetSignificantHistory() => _sessionLog.Where(a => a.IsSignificant).ToList().AsReadOnly();

    /// <summary>
    /// The most recent significant activity in the session log.
    /// Guaranteed to be non-null due to the Zero-Hollow Invariant.
    /// </summary>
    public SessionActivity LastActivity => _sessionLog
        .OrderByDescending(a => a.Timestamp)
        .First(a => a.IsSignificant);

    public Session(
        SessionId id,
        string remoteId,
        TaskDescription task,
        SourceContext source,
        SessionPulse pulse,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null,
        string? title = null,
        bool? requiresPlanApproval = null,
        AutomationMode mode = AutomationMode.Unspecified,
        Uri? dashboardUri = null,
        IEnumerable<SessionActivity>? history = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(remoteId);

        Id = id;
        RemoteId = remoteId;
        Task = task;
        Source = source;
        Pulse = pulse;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Title = title;
        RequiresPlanApproval = requiresPlanApproval;
        Mode = mode;
        DashboardUri = dashboardUri;

        // Initialize Log
        _sessionLog = history?.ToList() ?? new List<SessionActivity>();

        // Enforce Zero-Hollow Invariant 🏺
        // If the log is empty (new session) or contains no significant activities (rare edge case),
        // we must seed it.
        if (!_sessionLog.Any(a => a.IsSignificant))
        {
            var initialActivity = new SessionAssignedActivity(
                Guid.NewGuid().ToString(),
                "local-init",
                createdAt,
                ActivityOriginator.System,
                task);
            _sessionLog.Add(initialActivity);
        }

        RecordDomainEvent(new SessionAssigned(id, task));
    }

    /// <summary>
    /// Evaluates the agent's current state using the default policy.
    /// </summary>
    public SessionState State => new DefaultSessionStatePolicy().Evaluate(Pulse, SessionLog, IsDelivered);

    public bool IsDelivered => PullRequest != null;

    public void UpdatePulse(SessionPulse newPulse)
    {
        Pulse = newPulse;

        RecordDomainEvent(new StatusHeartbeatReceived(Id, newPulse));
    }

    /// <summary>
    /// Records a new collaborative activity and scans for delivered artifacts.
    /// </summary>
    public void AddActivity(SessionActivity activity)
    {
        _sessionLog.Add(activity);

        // Identify if this event produced a solution (ChangeSet)
        var changeSet = activity.Evidence.OfType<ChangeSet>().FirstOrDefault();
        if (changeSet != null)
        {
            SetSolution(changeSet);
        }
    }

    public void AddFeedback(string feedback, string activityId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(feedback);
        ArgumentException.ThrowIfNullOrWhiteSpace(activityId);
        // Note: activityId is the local ID for the new activity.
        // We use a temporary remote ID since this originates locally.
        AddActivity(new MessageActivity(activityId, "temp-remote-id", DateTimeOffset.UtcNow, ActivityOriginator.User, feedback));
    }

    public void SetPullRequest(PullRequest? pullRequest)
    {
        PullRequest = pullRequest;
    }

    /// <summary>
    /// Resolves the latest authoritative plan from the session history using a pluggable strategy.
    /// </summary>
    /// <param name="strategy">The strategy to use. Defaults to <see cref="TimestampBasedPlanResolutionStrategy"/> if null.</param>
    public PlanningActivity? GetLatestPlan(IPlanResolutionStrategy? strategy = null)
    {
        // Use default strategy if none provided (Pragmatic default for existing clients)
        var effectiveStrategy = strategy ?? new TimestampBasedPlanResolutionStrategy();

        return effectiveStrategy.ResolvePlan(_sessionLog);
    }

    private void SetSolution(ChangeSet solution)
    {
        Solution = solution;
        RecordDomainEvent(new SolutionReady(Id, solution));
    }

}
