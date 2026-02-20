using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence;

/// <summary>
/// A file-based implementation of the session writer port using folder-based storage.
/// </summary>
public sealed class RegistrySessionWriter : ISessionWriter
{
    private readonly IMetadataStore _metadataStore;
    private readonly IHistoryStore _historyStore;
    private readonly IRegistryTaskMapper _mapper;
    private readonly ISessionLayout _layout;
    private readonly IFileSystem _fileSystem;

    public RegistrySessionWriter(
        IMetadataStore metadataStore,
        IHistoryStore historyStore,
        IRegistryTaskMapper mapper,
        ISessionLayout layout,
        IFileSystem fileSystem)
    {
        _metadataStore = metadataStore;
        _historyStore = historyStore;
        _mapper = mapper;
        _layout = layout;
        _fileSystem = fileSystem;
    }

    public async Task RememberAsync(Session session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        var metadata = _mapper.MapToMetadataDto(session);
        await _metadataStore.SaveAsync(metadata, cancellationToken).ConfigureAwait(false);

        // If history file does not exist, persist current history (e.g. for new sessions).
        // We rely on ISessionArchivist for appending subsequent history.
        var historyPath = _layout.GetHistoryPath(session.Id);
        if (!_fileSystem.FileExists(historyPath) && session.SessionLog.Count > 0)
        {
            await _historyStore.AppendAsync(session.Id, session.SessionLog, cancellationToken).ConfigureAwait(false);
        }
    }

    public Task ForgetAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        var path = _layout.GetSessionDirectory(id);
        if (_fileSystem.DirectoryExists(path))
        {
            _fileSystem.DeleteDirectory(path, true);
        }
        return Task.CompletedTask;
    }
}
