using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// A portal for persisting and retrieving active Jules sessions.
/// </summary>
public interface ISessionRepository
{
    /// <summary>
    /// Saves a session to local storage.
    /// </summary>
    Task SaveAsync(Session session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific session by its identifier.
    /// </summary>
    Task<Session?> GetByIdAsync(SessionId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all active sessions.
    /// </summary>
    Task<IReadOnlyCollection<Session>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a session from local storage.
    /// </summary>
    Task DeleteAsync(SessionId id, CancellationToken cancellationToken = default);
}
