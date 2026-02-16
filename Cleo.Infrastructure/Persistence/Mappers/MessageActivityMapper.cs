using System.Text.Json;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence.Mappers;

internal sealed class MessageActivityMapper : IActivityPersistenceMapper
{
    private readonly ArtifactMapperFactory _artifactFactory;

    public MessageActivityMapper(ArtifactMapperFactory artifactFactory)
    {
        _artifactFactory = artifactFactory;
    }

    public string TypeKey => "MESSAGE";

    public bool CanHandle(SessionActivity activity) => activity is MessageActivity;

    public string SerializePayload(SessionActivity activity)
    {
        var message = (MessageActivity)activity;
        return JsonSerializer.Serialize(new MessagePayloadDto(
            message.RemoteId,
            message.Text,
            message.Evidence.Select(_artifactFactory.ToEnvelope).ToList()));
    }

    public SessionActivity DeserializePayload(string id, DateTimeOffset timestamp, ActivityOriginator originator, string json, string? executiveSummary)
    {
        var dto = JsonSerializer.Deserialize<MessagePayloadDto>(json) ?? throw new InvalidOperationException("Failed to deserialize payload.");

        return new MessageActivity(
            id, 
            dto.RemoteId ?? throw new InvalidOperationException("RemoteId is required."),
            timestamp, 
            originator,
            dto.Text ?? string.Empty,
            (dto.Evidence ?? []).Select(_artifactFactory.FromEnvelope).ToList(),
            executiveSummary);
    }

    private sealed record MessagePayloadDto(string? RemoteId, string Text, List<ArtifactEnvelope>? Evidence);
}
