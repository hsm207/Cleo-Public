using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// A remote source for fetching activities.
/// Responsible for retrieving activities from an external system.
/// </summary>
public interface IRemoteActivitySource
{
    Task<IReadOnlyCollection<SessionActivity>> FetchSinceAsync(SessionId id, RemoteFetchOptions options, CancellationToken cancellationToken = default);
}
