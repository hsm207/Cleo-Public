using System.Globalization;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps Jules API messaging activities into domain-centric MessageActivity objects.
/// </summary>
internal sealed class MessageActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.Payload is AgentMessagedPayload || dto.Payload is UserMessagedPayload;

    public SessionActivity Map(JulesActivityDto dto)
    {
        // Use the Metadata Originator as the source of truth, but fallback to payload type inference if needed
        var originator = ActivityOriginatorMapper.Map(dto.Metadata.Originator);
        
        var text = dto.Payload switch
        {
            AgentMessagedPayload amp => amp.AgentMessage,
            UserMessagedPayload ump => ump.UserMessage,
            _ => string.Empty
        };

        return new MessageActivity(dto.Metadata.Id, DateTimeOffset.Parse(dto.Metadata.CreateTime, CultureInfo.InvariantCulture), originator, text ?? string.Empty, ArtifactMappingHelper.MapArtifacts(dto.Metadata.Artifacts));
    }
}
