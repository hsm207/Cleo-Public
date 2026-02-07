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
    Task SendMessageAsync(SessionId id, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches the latest code solution (patch) produced in a session.
    /// </summary>
    /// <returns>The latest patch, or null if no solution is ready.</returns>
    Task<SolutionPatch?> GetLatestSolutionAsync(SessionId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the chronological history of messages in a session.
    /// </summary>
    Task<IReadOnlyCollection<ChatMessage>> GetConversationAsync(SessionId id, CancellationToken cancellationToken = default);
}
