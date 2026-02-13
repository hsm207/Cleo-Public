using System.Globalization;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps Jules API 'planGenerated' activities into domain-centric PlanningActivity objects.
/// </summary>
internal sealed class PlanningActivityMapper : IJulesActivityMapper<JulesPlanGeneratedPayloadDto>
{
    public SessionActivity Map(JulesActivityDto dto)
    {
        var payload = (JulesPlanGeneratedPayloadDto)dto.Payload;
        return new PlanningActivity(
            dto.Metadata.Name,
            dto.Metadata.Id, 
            DateTimeOffset.Parse(dto.Metadata.CreateTime, CultureInfo.InvariantCulture), 
            ActivityOriginatorMapper.Map(dto.Metadata.Originator),
            payload.Plan.Id ?? "unknown",
            payload.Plan.Steps?.Select(s => new PlanStep(s.Id, s.Index ?? 0, s.Title ?? string.Empty, s.Description ?? string.Empty)).ToList() ?? new List<PlanStep>(),
            ArtifactMappingHelper.MapArtifacts(dto.Metadata.Artifacts));
    }
}
