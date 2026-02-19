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

    public RegistrySessionWriter(
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

    public async Task RememberAsync(Session session, CancellationToken cancellationToken = default)
    {
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

    public async Task ForgetAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        var tasks = await LoadRegistryAsync(cancellationToken).ConfigureAwait(false);
        var count = tasks.RemoveAll(t => t.SessionId == id.Value);

        if (count > 0)
        {
            await SaveRegistryAsync(tasks, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<List<RegisteredSessionDto>> LoadRegistryAsync(CancellationToken ct)
    {
        var path = _pathProvider.GetRegistryPath();
        if (!_fileSystem.FileExists(path)) return new List<RegisteredSessionDto>();

        var json = await _fileSystem.ReadAllTextAsync(path, ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json)) return new List<RegisteredSessionDto>();

        return _serializer.Deserialize(json).ToList();
    }

    private async Task SaveRegistryAsync(IEnumerable<RegisteredSessionDto> tasks, CancellationToken ct)
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
