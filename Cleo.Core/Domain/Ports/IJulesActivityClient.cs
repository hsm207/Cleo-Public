using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// A portal for retrieving the chronological history of activities in a session.
/// </summary>
public interface IJulesActivityClient
{
    /// <summary>
    /// Lists the chronological history of activities in a session.
    /// </summary>
    Task<IReadOnlyCollection<SessionActivity>> GetActivitiesAsync(SessionId id, CancellationToken cancellationToken = default);
}
