using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Infrastructure.Persistence;

/// <summary>
/// A registry-based implementation of the session archivist.
/// Responsible for local storage and retrieval of session history using the registry.
/// </summary>
public sealed class RegistrySessionArchivist : ISessionArchivist
{
    private readonly ISessionReader _reader;
    private readonly ISessionWriter _writer;

    public RegistrySessionArchivist(ISessionReader reader, ISessionWriter writer)
    {
        _reader = reader;
        _writer = writer;
    }

    public async Task<IReadOnlyList<SessionActivity>> GetHistoryAsync(SessionId id, HistoryCriteria? criteria = null, CancellationToken cancellationToken = default)
    {
        var session = await _reader.RecallAsync(id, cancellationToken).ConfigureAwait(false);
        if (session == null)
        {
            return Array.Empty<SessionActivity>();
        }

        var history = session.SessionLog;

        if (criteria == null || criteria == HistoryCriteria.None)
        {
            return history.ToList().AsReadOnly();
        }

        return history.Where(criteria.IsSatisfiedBy).ToList().AsReadOnly();
    }

    public async Task AppendAsync(SessionId id, IEnumerable<SessionActivity> activities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(activities);

        // For registry implementation, we must load the full session, append, and save.
        var session = await _reader.RecallAsync(id, cancellationToken).ConfigureAwait(false);

        if (session == null)
        {
            // If session doesn't exist, we can't append history to it.
            // Caller should have ensured session exists (e.g. by creating it).
            throw new InvalidOperationException($"Session {id} not found in registry.");
        }

        bool modified = false;
        foreach (var activity in activities)
        {
            // Deduplication check: only add if not already present
            if (session.SessionLog.All(a => a.Id != activity.Id))
            {
                session.AddActivity(activity);
                modified = true;
            }
        }

        if (modified)
        {
            await _writer.RememberAsync(session, cancellationToken).ConfigureAwait(false);
        }
    }
}
