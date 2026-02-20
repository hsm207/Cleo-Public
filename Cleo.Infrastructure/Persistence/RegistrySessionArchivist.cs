using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence;

/// <summary>
/// A registry-based implementation of the session archivist.
/// Responsible for local storage and retrieval of session history using the registry.
/// </summary>
public sealed class RegistrySessionArchivist : ISessionArchivist
{
    private readonly ISessionReader _reader;
    private readonly ISessionWriter _writer;
    private readonly IHistoryStore _historyStore;

    public RegistrySessionArchivist(ISessionReader reader, ISessionWriter writer, IHistoryStore historyStore)
    {
        _reader = reader;
        _writer = writer;
        _historyStore = historyStore;
    }

    public async Task<IReadOnlyList<SessionActivity>> GetHistoryAsync(SessionId id, HistoryCriteria? criteria = null, CancellationToken cancellationToken = default)
    {
        // Use HistoryStore directly for efficient retrieval without loading full session.
        return await _historyStore.ReadAsync(id, criteria, cancellationToken).ConfigureAwait(false);
    }

    public async Task AppendAsync(SessionId id, IEnumerable<SessionActivity> activities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(activities);

        // Verify session exists (metadata check only would be better, but RecallAsync loads history now...)
        // Ideally we should have ExistsAsync on Reader or MetadataStore.
        // But for now, let's assume if we are appending, the session implies existence or we check via Reader.

        // Wait, RecallAsync is O(N) now because it loads history.
        // We want O(1).

        // We can check metadata existence via Writer? No.
        // We can just try to append.

        // But we need to ensure directory exists? IHistoryStore.AppendAsync handles directory creation (if parent exists).
        // It creates parent directory if missing.

        // So we can just append.

        await _historyStore.AppendAsync(id, activities, cancellationToken).ConfigureAwait(false);

        // Note: We are NOT updating Session UpdatedAt here.
        // If that's required, we need to update metadata.
    }
}
