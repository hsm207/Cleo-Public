using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// A portal for communicating with the remote Jules API.
/// </summary>
public interface IJulesClient
{
    /// <summary>
    /// Launches a new remote session for a specific task and source.
    /// </summary>
    Task<Session> CreateSessionAsync(TaskDescription task, SourceContext source, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the current state and heartbeat of a remote session.
    /// </summary>
    Task<SessionPulse> GetSessionPulseAsync(SessionId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a feedback message to Jules within an active session.
    /// </summary>
    Task SendMessageAsync(SessionId id, string feedback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the rich chronological history of activities in a session.
    /// </summary>
    Task<IReadOnlyCollection<SessionActivity>> GetActivitiesAsync(SessionId id, CancellationToken cancellationToken = default);
}
