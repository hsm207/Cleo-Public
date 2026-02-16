using System.Globalization;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps Jules API 'planApproved' activities into domain-centric ApprovalActivity objects.
/// </summary>
internal sealed class ApprovalActivityMapper : IJulesActivityMapper<JulesPlanApprovedPayloadDto>
{
    public SessionActivity Map(JulesActivityDto dto)
    {
        var payload = (JulesPlanApprovedPayloadDto)dto.Payload;
        return new ApprovalActivity(
            dto.Metadata.Name,
            dto.Metadata.Id, 
            DateTimeOffset.Parse(dto.Metadata.CreateTime, CultureInfo.InvariantCulture), 
            ActivityOriginatorMapper.Map(dto.Metadata.Originator),
            payload.PlanId ?? "unknown",
            ArtifactMappingHelper.MapArtifacts(dto.Metadata.Artifacts),
            dto.Metadata.Description); // RFC 016: Executive Summary
    }
}
