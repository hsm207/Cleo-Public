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
        return JsonSerializer.Serialize(new ProgressPayloadDto(
            progress.RemoteId,
            progress.Intent,
            progress.Reasoning,
            progress.Evidence.Select(_artifactFactory.ToEnvelope).ToList()));
    }

    public SessionActivity DeserializePayload(
        string id,
        DateTimeOffset timestamp,
        ActivityOriginator originator,
        string json,
        string? executiveSummary)
    {
        var dto = JsonSerializer.Deserialize<ProgressPayloadDto>(json) ?? throw new InvalidOperationException("Failed to deserialize payload.");

        return new ProgressActivity(
            id, 
            dto.RemoteId ?? throw new InvalidOperationException("RemoteId is required."),
            timestamp, 
            originator,
            dto.Intent,
            dto.Reasoning,
            (dto.Evidence ?? []).Select(_artifactFactory.FromEnvelope).ToList(),
            executiveSummary);
    }

    private sealed record ProgressPayloadDto(
        string? RemoteId,
        string Intent,
        string? Reasoning,
        List<ArtifactEnvelope>? Evidence);
}
