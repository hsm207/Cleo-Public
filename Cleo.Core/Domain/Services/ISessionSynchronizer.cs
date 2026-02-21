using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Services;

/// <summary>
/// Domain service responsible for synchronizing local session state with remote truth.
/// Handles watermark calculation, PR resolution, and activity merging.
/// </summary>
public interface ISessionSynchronizer
{
    /// <summary>
    /// Calculates the watermark (timestamp) for incremental fetching based on local history.
    /// </summary>
    DateTimeOffset? GetWatermark(Session? session);

    /// <summary>
    /// Synchronizes the local session with the remote session and new activities.
    /// Updates Pulse, Pull Request, and returns activities that were newly added to the local history.
    /// </summary>
    IReadOnlyList<SessionActivity> Synchronize(Session session, Session remoteSession, IEnumerable<SessionActivity> newActivities);
}
