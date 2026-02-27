using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence;

/// <summary>
/// A file-based implementation of the session reader port using folder-based storage.
/// </summary>
public sealed class RegistrySessionReader : ISessionReader
{
    private readonly IMetadataStore _metadataStore;
    private readonly IHistoryStore _historyStore;
    private readonly IRegistryTaskMapper _mapper;
    private readonly ISessionPathResolver _pathResolver;
    private readonly IFileSystem _fileSystem;

    public RegistrySessionReader(
        IMetadataStore metadataStore,
        IHistoryStore historyStore,
        IRegistryTaskMapper mapper,
        ISessionPathResolver pathResolver,
        IFileSystem fileSystem)
    {
        _metadataStore = metadataStore;
        _historyStore = historyStore;
        _mapper = mapper;
        _pathResolver = pathResolver;
        _fileSystem = fileSystem;
    }

    public async Task<Session?> RecallAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        var session = await RecallMetadataAsync(id, cancellationToken).ConfigureAwait(false);
        if (session == null) return null;

        await foreach (var activity in _historyStore.ReadAsync(id, null, cancellationToken).ConfigureAwait(false))
        {
            session.AddActivity(activity);
        }

        return session;
    }

    public async Task<IReadOnlyCollection<Session>> ListAsync(CancellationToken cancellationToken = default)
    {
        var root = _pathResolver.GetSessionsRoot();
        if (!_fileSystem.DirectoryExists(root)) return Array.Empty<Session>();

        var sessions = new List<Session>();
        var directories = _fileSystem.EnumerateDirectories(root);

        foreach (var dir in directories)
        {
            var folderName = Path.GetFileName(dir);
            var sessionId = new SessionId($"sessions/{folderName}");

            // O(1) Discovery: Only load metadata, skip history.
            var session = await RecallMetadataAsync(sessionId, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Registry integrity violation: Metadata missing for discovered session '{sessionId}'.");
            
            sessions.Add(session);
        }

        return sessions.AsReadOnly();
    }

    private async Task<Session?> RecallMetadataAsync(SessionId id, CancellationToken cancellationToken)
    {
        var metadata = await _metadataStore.LoadAsync(id, cancellationToken).ConfigureAwait(false);
        if (metadata == null) return null;

        return _mapper.MapFromMetadataDto(metadata);
    }
}
