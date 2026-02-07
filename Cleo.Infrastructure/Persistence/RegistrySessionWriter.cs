using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence;

/// <summary>
/// A file-based implementation of the session writer port.
/// </summary>
public sealed class RegistrySessionWriter : ISessionWriter
{
    private readonly IRegistryPathProvider _pathProvider;
    private readonly IRegistryTaskMapper _mapper;
    private readonly IRegistrySerializer _serializer;
    private readonly IFileSystem _fileSystem;

    public RegistrySessionWriter() : this(
        new DefaultRegistryPathProvider(),
        new RegistryTaskMapper(),
        new JsonRegistrySerializer(),
        new PhysicalFileSystem()) { }

    internal RegistrySessionWriter(
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

    public async Task SaveAsync(Session session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        var tasks = await LoadRegistryAsync(cancellationToken).ConfigureAwait(false);
        var dto = _mapper.MapToDto(session);

        var existing = tasks.FindIndex(t => t.SessionId == dto.SessionId);
        if (existing >= 0)
        {
            tasks[existing] = dto;
        }
        else
        {
            tasks.Add(dto);
        }

        await SaveRegistryAsync(tasks, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        var tasks = await LoadRegistryAsync(cancellationToken).ConfigureAwait(false);
        var count = tasks.RemoveAll(t => t.SessionId == id.Value);

        if (count > 0)
        {
            await SaveRegistryAsync(tasks, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<List<RegisteredTaskDto>> LoadRegistryAsync(CancellationToken ct)
    {
        var path = _pathProvider.GetRegistryPath();
        if (!_fileSystem.FileExists(path)) return new List<RegisteredTaskDto>();

        var json = await _fileSystem.ReadAllTextAsync(path, ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json)) return new List<RegisteredTaskDto>();

        return _serializer.Deserialize(json);
    }

    private async Task SaveRegistryAsync(List<RegisteredTaskDto> tasks, CancellationToken ct)
    {
        var path = _pathProvider.GetRegistryPath();
        var directory = Path.GetDirectoryName(path);
        
        if (!string.IsNullOrWhiteSpace(directory) && !_fileSystem.DirectoryExists(directory))
        {
            _fileSystem.CreateDirectory(directory);
        }

        var json = _serializer.Serialize(tasks);
        await _fileSystem.WriteAllTextAsync(path, json, ct).ConfigureAwait(false);
    }
}
