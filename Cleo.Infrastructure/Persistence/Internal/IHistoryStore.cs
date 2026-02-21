using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Infrastructure.Persistence.Internal;

public interface IHistoryStore
{
    Task AppendAsync(SessionId sessionId, IEnumerable<SessionActivity> activities, CancellationToken cancellationToken);
    IAsyncEnumerable<SessionActivity> ReadAsync(SessionId sessionId, HistoryCriteria? criteria, CancellationToken cancellationToken);
}
