using System.Globalization;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps Jules API 'planGenerated' activities into domain-centric PlanningActivity objects.
/// </summary>
internal sealed class PlanningActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.Payload is PlanGeneratedPayload;
    
    public SessionActivity Map(JulesActivityDto dto)
    {
        var payload = (PlanGeneratedPayload)dto.Payload;
        return new PlanningActivity(
            dto.Metadata.Id, 
            DateTimeOffset.Parse(dto.Metadata.CreateTime, CultureInfo.InvariantCulture), 
            ActivityOriginatorMapper.Map(dto.Metadata.Originator),
            payload.Plan.Id ?? "unknown",
            payload.Plan.Steps?.Select(s => new PlanStep(s.Index ?? 0, s.Title ?? string.Empty, s.Description ?? string.Empty)).ToList() ?? new List<PlanStep>(),
            ArtifactMappingHelper.MapArtifacts(dto.Metadata.Artifacts));
    }
}
