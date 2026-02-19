using System.Globalization;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps Jules API 'sessionFailed' activities into domain-centric FailureActivity objects.
/// </summary>
internal sealed class FailureActivityMapper : IJulesActivityMapper<JulesSessionFailedPayloadDto>
{
    public SessionActivity Map(JulesActivityDto dto)
    {
        var payload = (JulesSessionFailedPayloadDto)dto.Payload;
        return new FailureActivity(
            dto.Metadata.Name,
            dto.Metadata.Id,
            DateTimeOffset.Parse(dto.Metadata.CreateTime, CultureInfo.InvariantCulture),
            ActivityOriginatorMapper.Map(dto.Metadata.Originator),
            payload.Reason ?? "Unknown failure",
            ArtifactMappingHelper.MapArtifacts(dto.Metadata.Artifacts),
            dto.Metadata.Description);
    }
}
