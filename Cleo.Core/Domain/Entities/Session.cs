using Cleo.Core.Domain.Common;
using Cleo.Core.Domain.Events;
using Cleo.Core.Domain.Services;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Entities;

/// <summary>
/// The central authority for an autonomous coding collaboration.
/// </summary>
public class Session : AggregateRoot
{
    private readonly List<SessionActivity> _sessionLog = new();

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
        Uri? dashboardUri = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(remoteId);
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(pulse);

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

        RecordDomainEvent(new SessionAssigned(id, task));
    }

    /// <summary>
    /// Evaluates the agent's current stance, applying logical overrides for blocked sessions.
    /// </summary>
    public Stance EvaluatedStance
    {
        get
        {
            var physicalStance = MapToStance(Pulse.Status);

            // Logical Stance Override: If Idle + No PR + Last Activity was Planning -> AwaitingPlanApproval
            if (physicalStance == Stance.Idle && !IsDelivered && LastSignificantActivity is PlanningActivity)
            {
                return Stance.AwaitingPlanApproval;
            }

            return physicalStance;
        }
    }

    /// <summary>
    /// Evaluates the business truth of the session's deliverables.
    /// </summary>
    public DeliveryStatus DeliveryStatus
    {
        get
        {
            if (IsDelivered) return DeliveryStatus.Delivered;
            
            // If physically idle but no PR, it is officially unfulfilled.
            // This holds even if the EvaluatedStance is logically overridden to AwaitingPlanApproval.
            if (MapToStance(Pulse.Status) == Stance.Idle && !IsDelivered)
            {
                return DeliveryStatus.Unfulfilled;
            }

            var stance = EvaluatedStance;
            if (stance == Stance.Broken) return DeliveryStatus.Stalled;

            return DeliveryStatus.Pending;
        }
    }

    public bool IsDelivered => Solution != null || PullRequest != null;

    private SessionActivity? LastSignificantActivity => _sessionLog
        .OrderByDescending(a => a.Timestamp)
        .FirstOrDefault(a => a.IsSignificant);

    public void UpdatePulse(SessionPulse newPulse)
    {
        ArgumentNullException.ThrowIfNull(newPulse);
        Pulse = newPulse;

        RecordDomainEvent(new StatusHeartbeatReceived(Id, newPulse));

        if (newPulse.Status == SessionStatus.AwaitingFeedback)
        {
            RecordDomainEvent(new FeedbackRequested(Id, newPulse.Detail));
        }
    }

    /// <summary>
    /// Records a new collaborative activity and scans for delivered artifacts.
    /// </summary>
    public void AddActivity(SessionActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);
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

    public void SetPullRequest(PullRequest pullRequest)
    {
        ArgumentNullException.ThrowIfNull(pullRequest);
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

    private static Stance MapToStance(SessionStatus status) => status switch
    {
        SessionStatus.StartingUp => Stance.Queued,
        SessionStatus.Planning => Stance.Planning,
        SessionStatus.InProgress => Stance.Working,
        SessionStatus.Paused => Stance.Idle, // Paused is logically idle
        SessionStatus.AwaitingFeedback => Stance.AwaitingFeedback,
        SessionStatus.AwaitingPlanApproval => Stance.AwaitingPlanApproval,
        SessionStatus.Completed => Stance.Idle,
        SessionStatus.Abandoned => Stance.Idle,
        SessionStatus.Failed => Stance.Broken,
        // StateUnspecified or unknown values map to WTF 🚨
        _ => Stance.WTF
    };
}
