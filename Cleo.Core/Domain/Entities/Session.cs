using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Entities;

/// <summary>
/// The central authority for an autonomous coding collaboration.
/// </summary>
public class Session
{
    private readonly List<ChatMessage> _conversation = new();

    public SessionId Id { get; }
    public TaskDescription Task { get; }
    public SourceContext Source { get; }
    public SessionPulse Pulse { get; private set; }
    public SolutionPatch? Solution { get; private set; }
    public IReadOnlyCollection<ChatMessage> Conversation => _conversation.AsReadOnly();

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
    }

    public void UpdatePulse(SessionPulse newPulse)
    {
        ArgumentNullException.ThrowIfNull(newPulse);
        Pulse = newPulse;
    }

    public void AddMessage(ChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        _conversation.Add(message);
    }

    public void SetSolution(SolutionPatch solution)
    {
        ArgumentNullException.ThrowIfNull(solution);
        Solution = solution;
    }
}
