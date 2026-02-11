using System.Globalization;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps Jules API 'progressUpdated' activities into domain-centric ProgressActivity objects.
/// </summary>
internal sealed class ProgressActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.Payload.ProgressUpdated is not null;
    
    public SessionActivity Map(JulesActivityDto dto) => new ProgressActivity(
        dto.Metadata.Id, 
        DateTimeOffset.Parse(dto.Metadata.CreateTime, CultureInfo.InvariantCulture), 
        ActivityOriginatorMapper.Map(dto.Metadata.Originator),
        dto.Payload.ProgressUpdated!.Title ?? string.Empty,
        dto.Payload.ProgressUpdated.Description,
        ArtifactMappingHelper.MapArtifacts(dto.Metadata.Artifacts));
}
