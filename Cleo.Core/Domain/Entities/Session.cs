using Cleo.Core.Domain.Common;
using Cleo.Core.Domain.Events;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Entities;

/// <summary>
/// The central authority for an autonomous coding collaboration.
/// </summary>
public class Session : AggregateRoot
{
    private readonly List<SessionActivity> _sessionLog = new();

    public SessionId Id { get; }
    public TaskDescription Task { get; }
    public SourceContext Source { get; }
    public SessionPulse Pulse { get; private set; }
    public SolutionPatch? Solution { get; private set; }
    
    /// <summary>
    /// The authoritative, chronological ledger of everything that happened in this session.
    /// </summary>
    public IReadOnlyCollection<SessionActivity> SessionLog => _sessionLog.AsReadOnly();

    public Session(SessionId id, TaskDescription task, SourceContext source, SessionPulse pulse)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(pulse);

        Id = id;
        Task = task;
        Source = source;
        Pulse = pulse;

        RecordDomainEvent(new SessionAssigned(id, task));
    }

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
    /// Records a new collaborative activity in the session log.
    /// </summary>
    public void AddActivity(SessionActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);
        
        // Ensure activities are added in chronological order if possible
        // (In a distributed system we'd handle re-ordering, but here we keep it simple).
        _sessionLog.Add(activity);

        // Side Effect: If the activity is a Result, update the solution.
        if (activity is ResultActivity result)
        {
            SetSolution(result.Patch);
        }
    }

    /// <summary>
    /// Convenience method for adding user feedback to the log.
    /// </summary>
    public void AddFeedback(string feedback, string activityId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(feedback);
        AddActivity(new MessageActivity(activityId, DateTimeOffset.UtcNow, ActivityOriginator.User, feedback));
    }

    private void SetSolution(SolutionPatch solution)
    {
        Solution = solution;
        RecordDomainEvent(new SolutionReady(Id, solution));
    }
}
