using System.Text.Json;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence.Mappers;

internal sealed class FailureActivityMapper : IActivityPersistenceMapper
{
    private readonly ArtifactMapperFactory _artifactFactory;

    public FailureActivityMapper(ArtifactMapperFactory artifactFactory)
    {
        _artifactFactory = artifactFactory ?? throw new ArgumentNullException(nameof(artifactFactory));
    }

    public string TypeKey => "FAILED";

    public bool CanHandle(SessionActivity activity) => activity is FailureActivity;

    public string SerializePayload(SessionActivity activity)
    {
        var failure = (FailureActivity)activity;
        return JsonSerializer.Serialize(new FailurePayloadDto(
            failure.Reason,
            failure.Evidence.Select(_artifactFactory.ToEnvelope).ToList()));
    }

    public SessionActivity DeserializePayload(string id, DateTimeOffset timestamp, ActivityOriginator originator, string json)
    {
        var dto = JsonSerializer.Deserialize<FailurePayloadDto>(json);
        return new FailureActivity(
            id, 
            timestamp, 
            dto?.Reason ?? "Unknown failure",
            dto?.Evidence?.Select(_artifactFactory.FromEnvelope).ToList());
    }

    private sealed record FailurePayloadDto(string Reason, List<ArtifactEnvelope> Evidence);
}
