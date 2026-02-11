using System.Text.Json;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence.Mappers;

internal sealed class PlanningActivityMapper : IActivityPersistenceMapper
{
    private readonly ArtifactMapperFactory _artifactFactory;

    public PlanningActivityMapper(ArtifactMapperFactory artifactFactory)
    {
        _artifactFactory = artifactFactory ?? throw new ArgumentNullException(nameof(artifactFactory));
    }

    public string TypeKey => "PLAN_GENERATED";

    public bool CanHandle(SessionActivity activity) => activity is PlanningActivity;

    public string SerializePayload(SessionActivity activity)
    {
        var planning = (PlanningActivity)activity;
        var dto = new PlanningPayloadDto(
            planning.PlanId,
            planning.Steps.Select(s => new PlanStepDto(s.Index, s.Title, s.Description)).ToList(),
            planning.Evidence.Select(_artifactFactory.ToEnvelope).ToList());
        
        return JsonSerializer.Serialize(dto);
    }

    public SessionActivity DeserializePayload(string id, DateTimeOffset timestamp, ActivityOriginator originator, string json)
    {
        var dto = JsonSerializer.Deserialize<PlanningPayloadDto>(json);
        return new PlanningActivity(
            id, 
            timestamp, 
            originator,
            dto?.PlanId ?? "unknown",
            dto?.Steps?.Select(s => new PlanStep(s.Index, s.Title, s.Description)).ToList() ?? new List<PlanStep>(),
            dto?.Evidence?.Select(_artifactFactory.FromEnvelope).ToList());
    }

    private sealed record PlanningPayloadDto(string PlanId, List<PlanStepDto> Steps, List<ArtifactEnvelope> Evidence);
    private sealed record PlanStepDto(int Index, string Title, string Description);
}
