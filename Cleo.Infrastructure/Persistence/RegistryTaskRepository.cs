using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence;

/// <summary>
/// Provides a file-based implementation of the task repository, persisting session
/// data to a central registry using configurable pathing and serialization strategies.
/// </summary>
public sealed class RegistryTaskRepository : ISessionRepository
{
    private readonly IRegistryPathProvider _pathProvider;
    private readonly IRegistryTaskMapper _mapper;
    private readonly IRegistrySerializer _serializer;

    public RegistryTaskRepository() : this(
        new DefaultRegistryPathProvider(),
        new RegistryTaskMapper(),
        new JsonRegistrySerializer()) { }

    internal RegistryTaskRepository(
        IRegistryPathProvider pathProvider,
        IRegistryTaskMapper mapper,
        IRegistrySerializer serializer)
    {
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
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
        if (!File.Exists(path)) return new List<RegisteredTaskDto>();

        var json = await File.ReadAllTextAsync(path, ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json)) return new List<RegisteredTaskDto>();

        return _serializer.Deserialize(json);
    }

    private async Task SaveRegistryAsync(List<RegisteredTaskDto> tasks, CancellationToken ct)
    {
        var path = _pathProvider.GetRegistryPath();
        var directory = Path.GetDirectoryName(path);
        
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = _serializer.Serialize(tasks);
        await File.WriteAllTextAsync(path, json, ct).ConfigureAwait(false);
    }
}
