using System.Text.Json;
using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Infrastructure.Persistence.Internal;

// 1. The Path Provider (Where is the data?) 📍
public interface IRegistryPathProvider
{
    string GetRegistryPath();
}

internal sealed class DefaultRegistryPathProvider : IRegistryPathProvider
{
    private const string RegistryFileName = "tasks.json";
    public string GetRegistryPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "Cleo", RegistryFileName);
    }
}

// 2. The Task Mapper (What are we saving?) 🔄
public interface IRegistryTaskMapper
{
    RegisteredTaskDto MapToDto(Session session);
    Session MapToDomain(RegisteredTaskDto dto);
}

internal sealed class RegistryTaskMapper : IRegistryTaskMapper
{
    public RegisteredTaskDto MapToDto(Session session) => new(
        session.Id.Value,
        (string)session.Task,
        session.Source.Repository,
        session.Source.StartingBranch,
        session.Pulse.Status.ToString(),
        session.Pulse.Detail);

    public Session MapToDomain(RegisteredTaskDto dto) => new(
        new SessionId(dto.SessionId),
        (TaskDescription)dto.TaskDescription,
        new SourceContext(dto.Repository, dto.Branch),
        new SessionPulse(Enum.Parse<SessionStatus>(dto.Status), dto.Detail));
}

// 3. The Serializer (How do we format it?) 🔓
public interface IRegistrySerializer
{
    string Serialize(IEnumerable<RegisteredTaskDto> tasks);
    IEnumerable<RegisteredTaskDto> Deserialize(string content);
}

internal sealed class JsonRegistrySerializer : IRegistrySerializer
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    public string Serialize(IEnumerable<RegisteredTaskDto> tasks) => JsonSerializer.Serialize(tasks, Options);
    public IEnumerable<RegisteredTaskDto> Deserialize(string content) => 
        JsonSerializer.Deserialize<List<RegisteredTaskDto>>(content) ?? new List<RegisteredTaskDto>();
}

/// <summary>
/// A passive DTO for serializing a mission in the global Task Registry.
/// </summary>
public sealed record RegisteredTaskDto(
    string SessionId,
    string TaskDescription,
    string Repository,
    string Branch,
    string Status,
    string? Detail);
