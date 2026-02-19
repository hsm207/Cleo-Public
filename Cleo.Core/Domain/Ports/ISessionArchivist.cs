using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// The guardian of the Session Log. Responsible for local storage and retrieval of session history.
/// </summary>
public interface ISessionArchivist
{
    Task<IReadOnlyList<SessionActivity>> GetHistoryAsync(SessionId id, HistoryCriteria? criteria = null, CancellationToken cancellationToken = default);
    Task AppendAsync(SessionId id, IEnumerable<SessionActivity> activities, CancellationToken cancellationToken = default);
}
