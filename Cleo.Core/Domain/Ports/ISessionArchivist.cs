using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// The guardian of the Session Log. Responsible for retrieving the chronological history of a mission.
/// </summary>
public interface ISessionArchivist
{
    Task<IReadOnlyList<SessionActivity>> GetHistoryAsync(SessionId id, CancellationToken cancellationToken = default);
}
