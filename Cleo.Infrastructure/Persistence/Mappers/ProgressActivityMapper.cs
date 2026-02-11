using System.Text.Json;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence.Mappers;

internal sealed class ProgressActivityMapper : IActivityPersistenceMapper
{
    private readonly ArtifactMapperFactory _artifactFactory;

    public ProgressActivityMapper(ArtifactMapperFactory artifactFactory)
    {
        _artifactFactory = artifactFactory ?? throw new ArgumentNullException(nameof(artifactFactory));
    }

    public string TypeKey => "PROGRESS";

    public bool CanHandle(SessionActivity activity) => activity is ProgressActivity;

    public string SerializePayload(SessionActivity activity)
    {
        var progress = (ProgressActivity)activity;
        return JsonSerializer.Serialize(new ProgressPayloadDto(
            progress.RemoteId,
            progress.Title,
            progress.Description,
            progress.Evidence.Select(_artifactFactory.ToEnvelope).ToList()));
    }

    public SessionActivity DeserializePayload(string id, DateTimeOffset timestamp, ActivityOriginator originator, string json)
    {
        var dto = JsonSerializer.Deserialize<ProgressPayloadDto>(json);
        // Fallback RemoteId to id for legacy data
        var remoteId = dto?.RemoteId ?? id;

        return new ProgressActivity(
            id, 
            remoteId,
            timestamp, 
            originator,
            dto?.Title ?? string.Empty,
            dto?.Description,
            dto?.Evidence?.Select(_artifactFactory.FromEnvelope).ToList());
    }

    private sealed record ProgressPayloadDto(string? RemoteId, string Title, string? Description, List<ArtifactEnvelope> Evidence);
}
