using System.Text.Json;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence.Mappers;

internal sealed class ApprovalActivityMapper : IActivityPersistenceMapper
{
    private readonly ArtifactMapperFactory _artifactFactory;

    public ApprovalActivityMapper(ArtifactMapperFactory artifactFactory)
    {
        _artifactFactory = artifactFactory ?? throw new ArgumentNullException(nameof(artifactFactory));
    }

    public string TypeKey => "PLAN_APPROVED";

    public bool CanHandle(SessionActivity activity) => activity is ApprovalActivity;

    public string SerializePayload(SessionActivity activity)
    {
        var approval = (ApprovalActivity)activity;
        return JsonSerializer.Serialize(new ApprovalPayloadDto(
            approval.PlanId,
            approval.Evidence.Select(_artifactFactory.ToEnvelope).ToList()));
    }

    public SessionActivity DeserializePayload(string id, DateTimeOffset timestamp, ActivityOriginator originator, string json)
    {
        var dto = JsonSerializer.Deserialize<ApprovalPayloadDto>(json);
        return new ApprovalActivity(
            id, 
            timestamp, 
            originator,
            dto?.PlanId ?? "unknown",
            dto?.Evidence?.Select(_artifactFactory.FromEnvelope).ToList());
    }

    private sealed record ApprovalPayloadDto(string PlanId, List<ArtifactEnvelope> Evidence);
}
