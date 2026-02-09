using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// The observer of the session. Responsible for checking the Pulse (state) of a remote Session.
/// </summary>
public interface IPulseMonitor
{
    Task<SessionPulse> GetSessionPulseAsync(SessionId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the full, authoritative state of a remote session.
    /// </summary>
    Task<Session> GetRemoteSessionAsync(SessionId id, TaskDescription originalTask, CancellationToken cancellationToken = default);
}
