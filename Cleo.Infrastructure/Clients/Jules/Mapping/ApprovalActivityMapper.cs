using System.Globalization;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps Jules API 'planApproved' activities into domain-centric ApprovalActivity objects.
/// </summary>
internal sealed class ApprovalActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.Payload.PlanApproved is not null;
    
    public SessionActivity Map(JulesActivityDto dto) => new ApprovalActivity(
        dto.Metadata.Id, 
        DateTimeOffset.Parse(dto.Metadata.CreateTime, CultureInfo.InvariantCulture), 
        ActivityOriginatorMapper.Map(dto.Metadata.Originator),
        dto.Payload.PlanApproved!.PlanId ?? "unknown",
        ArtifactMappingHelper.MapArtifacts(dto.Metadata.Artifacts));
}
