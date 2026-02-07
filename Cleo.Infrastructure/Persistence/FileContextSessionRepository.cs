using System.Text.Json;
using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Infrastructure.Persistence;

/// <summary>
/// A lean implementation of the session repository that persists project context to a local .cleo directory.
/// </summary>
public sealed class FileContextSessionRepository : ISessionRepository
{
    private const string ContextDir = ".cleo";
    private const string ContextFile = "context.json";
    private readonly string _projectRoot;

    public FileContextSessionRepository(string projectRoot)
    {
        _projectRoot = projectRoot ?? throw new ArgumentNullException(nameof(projectRoot));
    }

    public async Task SaveAsync(Session session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        var contextPath = EnsureContextDirectory();
        
        var dto = LocalContextDto.FromDomain(session);
        var json = JsonSerializer.Serialize(dto);

        await File.WriteAllTextAsync(contextPath, json, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Session?> GetByIdAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        var contextPath = Path.Combine(_projectRoot, ContextDir, ContextFile);
        if (!File.Exists(contextPath)) return null;

        var json = await File.ReadAllTextAsync(contextPath, cancellationToken).ConfigureAwait(false);
        var dto = JsonSerializer.Deserialize<LocalContextDto>(json);

        if (dto == null || dto.SessionId != id.Value) return null;

        return dto.ToDomain();
    }

    public async Task<IReadOnlyCollection<Session>> ListAsync(CancellationToken cancellationToken = default)
    {
        // For a local project context, we usually only have one 'Active' session.
        // We list it if it exists.
        var contextPath = Path.Combine(_projectRoot, ContextDir, ContextFile);
        if (!File.Exists(contextPath)) return Array.Empty<Session>();

        var json = await File.ReadAllTextAsync(contextPath, cancellationToken).ConfigureAwait(false);
        var dto = JsonSerializer.Deserialize<LocalContextDto>(json);

        return dto != null ? new[] { dto.ToDomain() } : Array.Empty<Session>();
    }

    public Task DeleteAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        var contextPath = Path.Combine(_projectRoot, ContextDir, ContextFile);
        if (File.Exists(contextPath))
        {
            File.Delete(contextPath);
        }

        return Task.CompletedTask;
    }

    private string EnsureContextDirectory()
    {
        var dirPath = Path.Combine(_projectRoot, ContextDir);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        return Path.Combine(dirPath, ContextFile);
    }
}

/// <summary>
/// A DTO for serializing the local project context.
/// </summary>
internal sealed record LocalContextDto(
    string SessionId,
    string TaskDescription,
    string Repository,
    string Branch,
    string Status,
    string? Detail)
{
    public static LocalContextDto FromDomain(Session session) => new(
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
