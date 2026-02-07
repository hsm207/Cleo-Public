using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// A portal for persisting and removing active Jules sessions.
/// </summary>
public interface ISessionWriter
{
    /// <summary>
    /// Saves a session to local storage.
    /// </summary>
    Task SaveAsync(Session session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a session from local storage.
    /// </summary>
    Task DeleteAsync(SessionId id, CancellationToken cancellationToken = default);
}
