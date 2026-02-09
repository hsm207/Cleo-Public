using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps Jules API 'progressUpdated' activities into domain-centric ProgressActivity objects.
/// </summary>
internal sealed class ProgressActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.ProgressUpdated is not null;
    
    public SessionActivity Map(JulesActivityDto dto) => new ProgressActivity(
        dto.Id, 
        dto.CreateTime, 
        dto.ProgressUpdated!.Description ?? string.Empty,
        ArtifactMappingHelper.MapArtifacts(dto.Artifacts));
}
