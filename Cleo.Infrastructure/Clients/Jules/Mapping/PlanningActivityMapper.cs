using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps Jules API 'planGenerated' activities into domain-centric PlanningActivity objects.
/// </summary>
internal sealed class PlanningActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.PlanGenerated is not null;
    
    public SessionActivity Map(JulesActivityDto dto) => new PlanningActivity(
        dto.Id, 
        dto.CreateTime, 
        dto.PlanGenerated!.Plan.Id ?? "unknown",
        dto.PlanGenerated!.Plan.Steps.Select(s => new PlanStep(s.Index, s.Title, s.Description ?? string.Empty)).ToList());
}
