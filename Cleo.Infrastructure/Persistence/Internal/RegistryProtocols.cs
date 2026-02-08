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
    private const string RegistryFileName = "sessions.json";
    public string GetRegistryPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "Cleo", RegistryFileName);
    }
}

// 2. The Session Mapper (What are we saving?) 🔄
public interface IRegistryTaskMapper
{
    RegisteredSessionDto MapToDto(Session session);
    Session MapToDomain(RegisteredSessionDto dto);
}

internal sealed class RegistryTaskMapper : IRegistryTaskMapper
{
    public RegisteredSessionDto MapToDto(Session session) => new(
        session.Id.Value,
        (string)session.Task,
        session.Source.Repository,
        session.Source.StartingBranch,
        session.DashboardUri,
        session.SessionLog.Select(MapActivityToDto).ToList().AsReadOnly());

    public Session MapToDomain(RegisteredSessionDto dto)
    {
        var session = new Session(
            new SessionId(dto.SessionId),
            (TaskDescription)dto.TaskDescription,
            new SourceContext(dto.Repository, dto.Branch),
            new SessionPulse(SessionStatus.StartingUp), // Status is ephemeral!
            dto.DashboardUri);

        foreach (var activityDto in dto.History ?? Enumerable.Empty<RegisteredActivityDto>())
        {
            session.AddActivity(MapDtoToActivity(activityDto));
        }

        return session;
    }

    private static RegisteredActivityDto MapActivityToDto(SessionActivity activity) => new(
        activity.GetType().Name,
        activity.Id,
        activity.Timestamp,
        activity.Originator.ToString(),
        activity.GetContentSummary(),
        activity.GetMetaDetail());

    private static SessionActivity MapDtoToActivity(RegisteredActivityDto dto)
    {
        // For now, we reconstruct the activities using their summaries/details
        // In a full implementation, we would use a more robust polymorphic mapping
        return dto.Type switch
        {
            nameof(MessageActivity) => new MessageActivity(dto.Id, dto.Timestamp, Enum.Parse<ActivityOriginator>(dto.Originator), dto.Summary),
            nameof(ProgressActivity) => new ProgressActivity(dto.Id, dto.Timestamp, dto.Summary),
            nameof(CompletionActivity) => new CompletionActivity(dto.Id, dto.Timestamp),
            _ => new ProgressActivity(dto.Id, dto.Timestamp, dto.Summary) // Fallback
        };
    }
}

// 3. The Serializer (How do we format it?) 🔓
public interface IRegistrySerializer
{
    string Serialize(IEnumerable<RegisteredSessionDto> sessions);
    IEnumerable<RegisteredSessionDto> Deserialize(string content);
}

internal sealed class JsonRegistrySerializer : IRegistrySerializer
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    public string Serialize(IEnumerable<RegisteredSessionDto> sessions) => JsonSerializer.Serialize(sessions, Options);
    public IEnumerable<RegisteredSessionDto> Deserialize(string content) => 
        JsonSerializer.Deserialize<List<RegisteredSessionDto>>(content) ?? new List<RegisteredSessionDto>();
}

/// <summary>
/// A passive DTO for serializing a session in the global Session Registry.
/// </summary>
public sealed record RegisteredSessionDto(
    string SessionId,
    string TaskDescription,
    string Repository,
    string Branch,
    Uri? DashboardUri,
    IReadOnlyCollection<RegisteredActivityDto> History);

/// <summary>
/// A passive DTO for serializing a single activity in the session history.
/// </summary>
public sealed record RegisteredActivityDto(
    string Type,
    string Id,
    DateTimeOffset Timestamp,
    string Originator,
    string Summary,
    string? Detail);
