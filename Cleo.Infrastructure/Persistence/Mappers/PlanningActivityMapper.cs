using System.Text.Json;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence.Mappers;

internal sealed class PlanningActivityMapper : IActivityPersistenceMapper
{
    private readonly ArtifactMapperFactory _artifactFactory;

    public PlanningActivityMapper(ArtifactMapperFactory artifactFactory)
    {
        _artifactFactory = artifactFactory;
    }

    public string TypeKey => "PLAN_GENERATED";

    public bool CanHandle(SessionActivity activity) => activity is PlanningActivity;

    public string SerializePayload(SessionActivity activity)
    {
        var plan = (PlanningActivity)activity;
        return JsonSerializer.Serialize(new PlanningPayloadDto(
            plan.RemoteId,
            plan.PlanId,
            plan.Steps.Select(s => new PlanStepDto(s.Id, s.Index, s.Title, s.Description)).ToList(),
            plan.Evidence.Select(_artifactFactory.ToEnvelope).ToList()));
    }

    public SessionActivity DeserializePayload(string id, DateTimeOffset timestamp, ActivityOriginator originator, string json, string? executiveSummary)
    {
        var dto = JsonSerializer.Deserialize<PlanningPayloadDto>(json) ?? throw new InvalidOperationException("Failed to deserialize payload.");

        var steps = dto.Steps.Select(s => new PlanStep(s.Id, s.Index, s.Title, s.Description)).ToList();

        return new PlanningActivity(
            id, 
            dto.RemoteId ?? throw new InvalidOperationException("RemoteId is required."),
            timestamp, 
            originator,
            dto.PlanId ?? "unknown",
            steps,
            dto.Evidence.Select(_artifactFactory.FromEnvelope).ToList(),
            executiveSummary);
    }

    private sealed record PlanningPayloadDto(string? RemoteId, string PlanId, List<PlanStepDto> Steps, List<ArtifactEnvelope> Evidence);
    private sealed record PlanStepDto(string Id, int Index, string Title, string Description);
}
