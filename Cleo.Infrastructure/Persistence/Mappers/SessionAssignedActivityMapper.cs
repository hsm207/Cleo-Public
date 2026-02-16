using System.Text.Json;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence.Mappers;

internal sealed class SessionAssignedActivityMapper : IActivityPersistenceMapper
{
    private readonly ArtifactMapperFactory _artifactFactory;

    public SessionAssignedActivityMapper(ArtifactMapperFactory artifactFactory)
    {
        _artifactFactory = artifactFactory;
    }

    public string TypeKey => "SESSION_ASSIGNED";

    public bool CanHandle(SessionActivity activity) => activity is SessionAssignedActivity;

    public string SerializePayload(SessionActivity activity)
    {
        var assigned = (SessionAssignedActivity)activity;
        return JsonSerializer.Serialize(new SessionAssignedPayloadDto(
            assigned.RemoteId,
            (string)assigned.Task,
            assigned.Evidence.Select(_artifactFactory.ToEnvelope).ToList()));
    }

    public SessionActivity DeserializePayload(string id, DateTimeOffset timestamp, ActivityOriginator originator, string json, string? executiveSummary)
    {
        var dto = JsonSerializer.Deserialize<SessionAssignedPayloadDto>(json);
        // Fallback RemoteId to id for legacy data
        var remoteId = dto?.RemoteId ?? id;

        return new SessionAssignedActivity(
            id,
            remoteId,
            timestamp,
            originator,
            (TaskDescription)(dto?.TaskDescription ?? "Unknown Task"),
            dto?.Evidence?.Select(_artifactFactory.FromEnvelope).ToList(),
            executiveSummary);
    }

    private sealed record SessionAssignedPayloadDto(string? RemoteId, string TaskDescription, List<ArtifactEnvelope> Evidence);
}
