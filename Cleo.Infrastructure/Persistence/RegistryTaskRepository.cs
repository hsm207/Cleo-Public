using System.Text.Json;
using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Infrastructure.Persistence;

/// <summary>
/// A global implementation of the task repository that persists all active missions
/// to a central registry in the user's local application data directory.
/// </summary>
public sealed class RegistryTaskRepository : ISessionRepository
{
    private const string RegistryFileName = "tasks.json";
    private readonly string _registryPath;

    private static readonly JsonSerializerOptions Options = new() 
    { 
        WriteIndented = true 
    };

    public RegistryTaskRepository() : this(GetDefaultRegistryPath()) { }

    internal RegistryTaskRepository(string registryPath)
    {
        _registryPath = registryPath ?? throw new ArgumentNullException(nameof(registryPath));
    }

    public async Task SaveAsync(Session session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        var tasks = await LoadRegistryAsync(cancellationToken).ConfigureAwait(false);
        var dto = RegisteredTaskDto.FromDomain(session);

        // Update existing or add new
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

        return dto?.ToDomain();
    }

    public async Task<IReadOnlyCollection<Session>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await LoadRegistryAsync(cancellationToken).ConfigureAwait(false);
        return tasks.Select(t => t.ToDomain()).ToList().AsReadOnly();
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
        if (!File.Exists(_registryPath)) return new List<RegisteredTaskDto>();

        var json = await File.ReadAllTextAsync(_registryPath, ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json)) return new List<RegisteredTaskDto>();

        return JsonSerializer.Deserialize<List<RegisteredTaskDto>>(json) ?? new List<RegisteredTaskDto>();
    }

    private async Task SaveRegistryAsync(List<RegisteredTaskDto> tasks, CancellationToken ct)
    {
        var directory = Path.GetDirectoryName(_registryPath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(tasks, Options);
        await File.WriteAllTextAsync(_registryPath, json, ct).ConfigureAwait(false);
    }

    private static string GetDefaultRegistryPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "Cleo", RegistryFileName);
    }
}

/// <summary>
/// A DTO for serializing a mission in the global Task Registry.
/// </summary>
internal sealed record RegisteredTaskDto(
    string SessionId,
    string TaskDescription,
    string Repository,
    string Branch,
    string Status,
    string? Detail)
{
    public static RegisteredTaskDto FromDomain(Session session) => new(
        session.Id.Value,
        (string)session.Task,
        session.Source.Repository,
        session.Source.StartingBranch,
        session.Pulse.Status.ToString(),
        session.Pulse.Detail);

    public Session ToDomain() => new(
        new SessionId(SessionId),
        (TaskDescription)TaskDescription,
        new SourceContext(Repository, Branch),
        new SessionPulse(Enum.Parse<SessionStatus>(Status), Detail));
}
