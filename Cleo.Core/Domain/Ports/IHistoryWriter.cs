using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// A focused port for appending to session history.
/// </summary>
public interface IHistoryWriter
{
    Task AppendAsync(SessionId id, IEnumerable<SessionActivity> activities, CancellationToken cancellationToken = default);
}
