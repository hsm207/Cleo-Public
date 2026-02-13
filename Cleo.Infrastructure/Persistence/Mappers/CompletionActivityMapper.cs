using System.Text.Json;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence.Mappers;

internal sealed class CompletionActivityMapper : IActivityPersistenceMapper
{
    private readonly ArtifactMapperFactory _artifactFactory;

    public CompletionActivityMapper(ArtifactMapperFactory artifactFactory)
    {
        _artifactFactory = artifactFactory;
    }

    public string TypeKey => "COMPLETED";

    public bool CanHandle(SessionActivity activity) => activity is CompletionActivity;

    public string SerializePayload(SessionActivity activity)
    {
        var completed = (CompletionActivity)activity;
        return JsonSerializer.Serialize(new CompletionPayloadDto(
            completed.RemoteId,
            completed.Evidence.Select(_artifactFactory.ToEnvelope).ToList()));
    }

    public SessionActivity DeserializePayload(string id, DateTimeOffset timestamp, ActivityOriginator originator, string json)
    {
        var dto = JsonSerializer.Deserialize<CompletionPayloadDto>(json);
        var remoteId = dto?.RemoteId ?? id;

        return new CompletionActivity(
            id,
            remoteId,
            timestamp,
            originator,
            dto?.Evidence?.Select(_artifactFactory.FromEnvelope).ToList());
    }

    private sealed record CompletionPayloadDto(string? RemoteId, List<ArtifactEnvelope> Evidence);
}
