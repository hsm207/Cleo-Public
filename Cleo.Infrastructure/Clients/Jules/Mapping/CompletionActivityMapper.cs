using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps Jules API 'sessionCompleted' activities into domain-centric CompletionActivity objects.
/// </summary>
internal sealed class CompletionActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.SessionCompleted is not null;
    
    public SessionActivity Map(JulesActivityDto dto) => new CompletionActivity(dto.Id, dto.CreateTime, ArtifactMappingHelper.MapArtifacts(dto.Artifacts));
}
