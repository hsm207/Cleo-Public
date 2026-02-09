using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps Jules API messaging activities into domain-centric MessageActivity objects.
/// </summary>
internal sealed class MessageActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.AgentMessaged is not null || dto.UserMessaged is not null;

    public SessionActivity Map(JulesActivityDto dto)
    {
        var originator = dto.AgentMessaged is not null ? ActivityOriginator.Agent : ActivityOriginator.User;
        var text = dto.AgentMessaged?.AgentMessage ?? dto.UserMessaged?.UserMessage ?? string.Empty;

        return new MessageActivity(dto.Id, dto.CreateTime, originator, text, ArtifactMappingHelper.MapArtifacts(dto.Artifacts));
    }
}
