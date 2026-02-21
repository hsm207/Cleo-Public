using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// A focused port for reading session history.
/// </summary>
public interface IHistoryReader
{
    Task<IReadOnlyList<SessionActivity>> GetHistoryAsync(SessionId id, HistoryCriteria? criteria = null, CancellationToken cancellationToken = default);
}
