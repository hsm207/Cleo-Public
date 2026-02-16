using System.Globalization;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps Jules API 'agentMessaged' activities into domain-centric MessageActivity objects.
/// </summary>
internal sealed class AgentMessageActivityMapper : IJulesActivityMapper<JulesAgentMessagedPayloadDto>
{
    public SessionActivity Map(JulesActivityDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var payload = (JulesAgentMessagedPayloadDto)dto.Payload;
        var originator = ActivityOriginatorMapper.Map(dto.Metadata.Originator);

        return new MessageActivity(
            dto.Metadata.Name,
            dto.Metadata.Id,
            DateTimeOffset.Parse(dto.Metadata.CreateTime, CultureInfo.InvariantCulture),
            originator,
            payload.AgentMessage ?? string.Empty,
            ArtifactMappingHelper.MapArtifacts(dto.Metadata.Artifacts),
            dto.Metadata.Description); // RFC 016: Executive Summary
    }
}
