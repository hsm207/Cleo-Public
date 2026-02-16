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

    public string TypeKey => "COMPLETION";

    public bool CanHandle(SessionActivity activity) => activity is CompletionActivity;

    public string SerializePayload(SessionActivity activity)
    {
        var completion = (CompletionActivity)activity;
        return JsonSerializer.Serialize(new CompletionPayloadDto(
            completion.RemoteId,
            completion.Evidence.Select(_artifactFactory.ToEnvelope).ToList()));
    }

    public SessionActivity DeserializePayload(string id, DateTimeOffset timestamp, ActivityOriginator originator, string json, string? executiveSummary)
    {
        var dto = JsonSerializer.Deserialize<CompletionPayloadDto>(json);
        // Fallback RemoteId to id for legacy data
        var remoteId = dto?.RemoteId ?? id;

        return new CompletionActivity(
            id,
            remoteId,
            timestamp,
            originator,
            dto?.Evidence?.Select(_artifactFactory.FromEnvelope).ToList(),
            executiveSummary);
    }

    private sealed record CompletionPayloadDto(string? RemoteId, List<ArtifactEnvelope> Evidence);
}
