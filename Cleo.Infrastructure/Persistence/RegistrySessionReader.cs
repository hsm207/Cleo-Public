using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence;

/// <summary>
/// A file-based implementation of the session reader port.
/// </summary>
public sealed class RegistrySessionReader : ISessionReader
{
    private readonly IRegistryPathProvider _pathProvider;
    private readonly IRegistryTaskMapper _mapper;
    private readonly IRegistrySerializer _serializer;
    private readonly IFileSystem _fileSystem;

    public RegistrySessionReader(
        IRegistryPathProvider pathProvider,
        IRegistryTaskMapper mapper,
        IRegistrySerializer serializer,
        IFileSystem fileSystem)
    {
        _pathProvider = pathProvider;
        _mapper = mapper;
        _serializer = serializer;
        _fileSystem = fileSystem;
    }

    public async Task<Session?> RecallAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        var tasks = await LoadRegistryAsync(cancellationToken).ConfigureAwait(false);
        var dto = tasks.FirstOrDefault(t => t.SessionId == id.Value);

        return dto != null ? _mapper.MapToDomain(dto) : null;
    }

    public async Task<IReadOnlyCollection<Session>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await LoadRegistryAsync(cancellationToken).ConfigureAwait(false);
        return tasks.Select(_mapper.MapToDomain).ToList().AsReadOnly();
    }

    private async Task<IEnumerable<RegisteredSessionDto>> LoadRegistryAsync(CancellationToken ct)
    {
        var path = _pathProvider.GetRegistryPath();
        if (!_fileSystem.FileExists(path)) return Array.Empty<RegisteredSessionDto>();

        var json = await _fileSystem.ReadAllTextAsync(path, ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json)) return Array.Empty<RegisteredSessionDto>();

        return _serializer.Deserialize(json);
    }
}
