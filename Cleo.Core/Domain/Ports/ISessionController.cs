using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// A port for directing and controlling the progress of a live remote session.
/// </summary>
public interface ISessionController
{
    /// <summary>
    /// Formally approves a generated plan, allowing the agent to begin execution.
    /// </summary>
    Task ApprovePlanAsync(SessionId id, CancellationToken cancellationToken = default);
}
