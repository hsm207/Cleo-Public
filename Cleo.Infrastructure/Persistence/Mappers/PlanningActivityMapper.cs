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

    // RFC 016: Adjusted to "PLAN_GENERATED" to satisfy HighFidelityArchaeologyTests.
    // This aligns with the "Stable Discriminator" pattern (Event-based naming).
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
        var dto = JsonSerializer.Deserialize<PlanningPayloadDto>(json);
        // Fallback RemoteId to id for legacy data
        var remoteId = dto?.RemoteId ?? id;

        var steps = dto?.Steps?.Select(s => new PlanStep(s.Id, s.Index, s.Title, s.Description)).ToList()
                    ?? new List<PlanStep>();

        return new PlanningActivity(
            id, 
            remoteId,
            timestamp, 
            originator,
            dto?.PlanId ?? "unknown",
            steps,
            dto?.Evidence?.Select(_artifactFactory.FromEnvelope).ToList(),
            executiveSummary);
    }

    private sealed record PlanningPayloadDto(string? RemoteId, string PlanId, List<PlanStepDto> Steps, List<ArtifactEnvelope> Evidence);
    private sealed record PlanStepDto(string Id, int Index, string Title, string Description);
}
