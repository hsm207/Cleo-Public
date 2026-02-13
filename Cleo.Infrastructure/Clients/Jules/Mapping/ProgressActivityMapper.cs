using System.Globalization;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps Jules API 'progressUpdated' activities into domain-centric ProgressActivity objects.
/// </summary>
internal sealed class ProgressActivityMapper : IJulesActivityMapper<JulesProgressUpdatedPayloadDto>
{
    public SessionActivity Map(JulesActivityDto dto)
    {
        var payload = (JulesProgressUpdatedPayloadDto)dto.Payload;

        // RFC 009: Narrative Intelligence
        // The API 'Title' maps to Domain 'Intent' (Title)
        // The API 'Description' maps to Domain 'Thought' (Description)
        return new ProgressActivity(
            dto.Metadata.Name,
            dto.Metadata.Id, 
            DateTimeOffset.Parse(dto.Metadata.CreateTime, CultureInfo.InvariantCulture), 
            ActivityOriginatorMapper.Map(dto.Metadata.Originator),
            payload.Title ?? string.Empty,
            payload.Description, // This captures the internal monologue 🧠
            ArtifactMappingHelper.MapArtifacts(dto.Metadata.Artifacts));
    }
}
