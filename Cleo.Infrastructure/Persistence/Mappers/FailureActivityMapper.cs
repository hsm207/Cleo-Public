using System.Text.Json;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence.Mappers;

internal sealed class FailureActivityMapper : IActivityPersistenceMapper
{
    private readonly ArtifactMapperFactory _artifactFactory;

    public FailureActivityMapper(ArtifactMapperFactory artifactFactory)
    {
        _artifactFactory = artifactFactory;
    }

    public string TypeKey => "FAILURE";

    public bool CanHandle(SessionActivity activity) => activity is FailureActivity;

    public string SerializePayload(SessionActivity activity)
    {
        var failure = (FailureActivity)activity;
        return JsonSerializer.Serialize(new FailurePayloadDto(
            failure.RemoteId,
            failure.Reason,
            failure.Evidence.Select(_artifactFactory.ToEnvelope).ToList()));
    }

    public SessionActivity DeserializePayload(string id, DateTimeOffset timestamp, ActivityOriginator originator, string json, string? executiveSummary)
    {
        var dto = JsonSerializer.Deserialize<FailurePayloadDto>(json) ?? throw new InvalidOperationException("Failed to deserialize payload.");

        return new FailureActivity(
            id, 
            dto.RemoteId ?? throw new InvalidOperationException("RemoteId is required."),
            timestamp, 
            originator,
            dto.Reason ?? "Unknown failure",
            (dto.Evidence ?? []).Select(_artifactFactory.FromEnvelope).ToList(),
            executiveSummary);
    }

    private sealed record FailurePayloadDto(string? RemoteId, string Reason, List<ArtifactEnvelope>? Evidence);
}
