using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// A symmetric, action-oriented port for recalling and listing active Jules sessions.
/// Acts as the query mechanism for Cleo's Workbench Memory. 🕵️‍♀️🔍
/// </summary>
public interface ISessionReader
{
    /// <summary>
    /// Recalls a specific session from the local registry by its unique handle.
    /// </summary>
    Task<Session?> RecallAsync(SessionId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a list of all sessions currently tracked in the local registry.
    /// </summary>
    Task<IReadOnlyCollection<Session>> ListAsync(CancellationToken cancellationToken = default);
}
