using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// A portal for retrieving active Jules sessions.
/// </summary>
public interface ISessionReader
{
    /// <summary>
    /// Retrieves a specific session by its identifier.
    /// </summary>
    Task<Session?> GetByIdAsync(SessionId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all active sessions.
    /// </summary>
    Task<IReadOnlyCollection<Session>> ListAsync(CancellationToken cancellationToken = default);
}
