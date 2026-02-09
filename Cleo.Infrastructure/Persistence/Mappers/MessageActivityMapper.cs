using System.Text.Json;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence.Mappers;

internal sealed class MessageActivityMapper : IActivityPersistenceMapper
{
    private readonly ArtifactMapperFactory _artifactFactory;

    public MessageActivityMapper(ArtifactMapperFactory artifactFactory)
    {
        _artifactFactory = artifactFactory ?? throw new ArgumentNullException(nameof(artifactFactory));
    }

    public string TypeKey => "MESSAGE";

    public bool CanHandle(SessionActivity activity) => activity is MessageActivity;

    public string SerializePayload(SessionActivity activity)
    {
        var message = (MessageActivity)activity;
        return JsonSerializer.Serialize(new MessagePayloadDto(
            message.Text,
            message.Evidence.Select(_artifactFactory.ToEnvelope).ToList()));
    }

    public SessionActivity DeserializePayload(string id, DateTimeOffset timestamp, ActivityOriginator originator, string json)
    {
        var dto = JsonSerializer.Deserialize<MessagePayloadDto>(json);
        return new MessageActivity(
            id, 
            timestamp, 
            originator, 
            dto?.Text ?? string.Empty,
            dto?.Evidence?.Select(_artifactFactory.FromEnvelope).ToList());
    }

    private sealed record MessagePayloadDto(string Text, List<ArtifactEnvelope> Evidence);
}
