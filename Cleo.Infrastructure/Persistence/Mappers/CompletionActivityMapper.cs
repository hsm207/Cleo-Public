using System.Text.Json;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence.Mappers;

internal sealed class CompletionActivityMapper : IActivityPersistenceMapper
{
    private readonly ArtifactMapperFactory _artifactFactory;

    public CompletionActivityMapper(ArtifactMapperFactory artifactFactory)
    {
        _artifactFactory = artifactFactory ?? throw new ArgumentNullException(nameof(artifactFactory));
    }

    public string TypeKey => "COMPLETED";

    public bool CanHandle(SessionActivity activity) => activity is CompletionActivity;

    public string SerializePayload(SessionActivity activity)
    {
        var completed = (CompletionActivity)activity;
        return JsonSerializer.Serialize(new CompletionPayloadDto(
            completed.Evidence.Select(_artifactFactory.ToEnvelope).ToList()));
    }

        public SessionActivity DeserializePayload(string id, DateTimeOffset timestamp, ActivityOriginator originator, string json)

        {

            var dto = JsonSerializer.Deserialize<CompletionPayloadDto>(json);

            return new CompletionActivity(id, timestamp, originator, dto?.Evidence?.Select(_artifactFactory.FromEnvelope).ToList());

        }

    private sealed record CompletionPayloadDto(List<ArtifactEnvelope> Evidence);
}
