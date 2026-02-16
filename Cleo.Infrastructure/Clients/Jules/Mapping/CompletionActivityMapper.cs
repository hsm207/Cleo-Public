using System.Globalization;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps Jules API 'sessionCompleted' activities into domain-centric CompletionActivity objects.
/// </summary>
internal sealed class CompletionActivityMapper : IJulesActivityMapper<JulesSessionCompletedPayloadDto>
{
    public SessionActivity Map(JulesActivityDto dto) => new CompletionActivity(
        dto.Metadata.Name,
        dto.Metadata.Id, 
        DateTimeOffset.Parse(dto.Metadata.CreateTime, CultureInfo.InvariantCulture), 
        ActivityOriginatorMapper.Map(dto.Metadata.Originator),
        ArtifactMappingHelper.MapArtifacts(dto.Metadata.Artifacts),
        dto.Metadata.Description);
}
