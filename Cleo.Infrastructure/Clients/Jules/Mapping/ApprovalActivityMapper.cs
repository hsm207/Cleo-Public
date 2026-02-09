using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps Jules API 'planApproved' activities into domain-centric ApprovalActivity objects.
/// </summary>
internal sealed class ApprovalActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.PlanApproved is not null;
    
    public SessionActivity Map(JulesActivityDto dto) => new ApprovalActivity(
        dto.Id, 
        dto.CreateTime, 
        dto.PlanApproved!.PlanId ?? "unknown",
        ArtifactMappingHelper.MapArtifacts(dto.Artifacts));
}
