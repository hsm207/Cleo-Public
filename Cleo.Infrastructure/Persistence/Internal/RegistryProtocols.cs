using System.Text.Json;
using System.Text.Json.Serialization;
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
/// A high-fidelity DTO for serializing a session in the global Session Registry.
/// Following the High-Fidelity Ledger pattern.
/// </summary>
public sealed record RegisteredSessionDto(
    string SessionId,
    string TaskDescription,
    string Repository,
    string SourceBranch,
    SessionStatus PulseStatus,
    Uri? DashboardUri,
    IReadOnlyCollection<ActivityEnvelopeDto> History,
    RegisteredPullRequestDto? PullRequest = null);

public sealed record RegisteredPullRequestDto(
    Uri Url,
    string Title,
    string Description,
    string HeadRef,
    string BaseRef);
