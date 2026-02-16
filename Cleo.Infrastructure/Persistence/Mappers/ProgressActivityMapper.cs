using System.Text.Json;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence.Mappers;

internal sealed class ProgressActivityMapper : IActivityPersistenceMapper
{
    private readonly ArtifactMapperFactory _artifactFactory;

    public ProgressActivityMapper(ArtifactMapperFactory artifactFactory)
    {
        _artifactFactory = artifactFactory;
    }

    public string TypeKey => "PROGRESS";

    public bool CanHandle(SessionActivity activity) => activity is ProgressActivity;

    public string SerializePayload(SessionActivity activity)
    {
        var progress = (ProgressActivity)activity;
        // RFC 016: Absolute Transformer & Signal Recovery 🛰️💎
        // Renamed DTO fields to match Domain Language (Breaking Change!)
        return JsonSerializer.Serialize(new ProgressPayloadDto(
            progress.RemoteId,
            progress.Intent,
            progress.Reasoning,
            progress.Evidence.Select(_artifactFactory.ToEnvelope).ToList()));
            // Note: ExecutiveSummary is now persisted in the Envelope, not the Payload! 👸💎
    }

    public SessionActivity DeserializePayload(
        string id,
        DateTimeOffset timestamp,
        ActivityOriginator originator,
        string json,
        string? executiveSummary) // Injected from Envelope
    {
        var dto = JsonSerializer.Deserialize<ProgressPayloadDto>(json);
        // Fallback RemoteId to id for legacy data
        var remoteId = dto?.RemoteId ?? id;

        return new ProgressActivity(
            id, 
            remoteId,
            timestamp, 
            originator,
            dto?.Intent ?? string.Empty,
            dto?.Reasoning, // Restores the agent's thought
            dto?.Evidence?.Select(_artifactFactory.FromEnvelope).ToList(),
            executiveSummary); // Hydrated from Envelope
    }

    // RFC 016: DTO Updated to reflect Domain Language (Intent/Reasoning)
    private sealed record ProgressPayloadDto(
        string? RemoteId,
        string Intent,
        string? Reasoning,
        List<ArtifactEnvelope> Evidence);
}
