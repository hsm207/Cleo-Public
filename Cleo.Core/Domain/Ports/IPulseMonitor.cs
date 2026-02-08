using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// The observer of the mission. Responsible for checking the Pulse (state) of a remote Session.
/// </summary>
public interface IPulseMonitor
{
    Task<SessionPulse> GetSessionPulseAsync(SessionId id, CancellationToken cancellationToken = default);
}
