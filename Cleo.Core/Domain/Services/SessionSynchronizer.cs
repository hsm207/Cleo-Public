using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Services;

public sealed class SessionSynchronizer : ISessionSynchronizer
{
    private readonly IPrResolver _prResolver;

    public SessionSynchronizer(IPrResolver prResolver)
    {
        _prResolver = prResolver;
    }

    public DateTimeOffset? GetWatermark(Session? session)
    {
        if (session == null || session.SessionLog.Count == 0)
        {
            return null;
        }
        return session.SessionLog.Max(a => a.Timestamp);
    }

    public void Synchronize(Session session, Session remoteSession, IEnumerable<SessionActivity> newActivities)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(remoteSession);
        ArgumentNullException.ThrowIfNull(newActivities);

        // 1. Sync Pulse (Heartbeat)
        session.UpdatePulse(remoteSession.Pulse);

        // 2. Resolve Pull Request (Remote First)
        var resolvedPr = _prResolver.Resolve(session.PullRequest, remoteSession.PullRequest);
        session.SetPullRequest(resolvedPr);

        // 3. Append New Activities (Deduplication)
        foreach (var activity in newActivities)
        {
            // Simple synchronization: Add only if not already present
            if (session.SessionLog.All(a => a.Id != activity.Id))
            {
                session.AddActivity(activity);
            }
        }
    }
}
