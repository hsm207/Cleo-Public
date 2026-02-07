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

    public RegistrySessionReader() : this(
        new DefaultRegistryPathProvider(),
        new RegistryTaskMapper(),
        new JsonRegistrySerializer(),
        new PhysicalFileSystem()) { }

    internal RegistrySessionReader(
        IRegistryPathProvider pathProvider,
        IRegistryTaskMapper mapper,
        IRegistrySerializer serializer,
        IFileSystem fileSystem)
    {
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task<Session?> GetByIdAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        var tasks = await LoadRegistryAsync(cancellationToken).ConfigureAwait(false);
        var dto = tasks.FirstOrDefault(t => t.SessionId == id.Value);

        return dto != null ? _mapper.MapToDomain(dto) : null;
    }

    public async Task<IReadOnlyCollection<Session>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await LoadRegistryAsync(cancellationToken).ConfigureAwait(false);
        return tasks.Select(_mapper.MapToDomain).ToList().AsReadOnly();
    }

    private async Task<List<RegisteredTaskDto>> LoadRegistryAsync(CancellationToken ct)
    {
        var path = _pathProvider.GetRegistryPath();
        if (!_fileSystem.FileExists(path)) return new List<RegisteredTaskDto>();

        var json = await _fileSystem.ReadAllTextAsync(path, ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json)) return new List<RegisteredTaskDto>();

        return _serializer.Deserialize(json);
    }
}
