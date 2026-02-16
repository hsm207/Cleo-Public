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

        // RFC 016: Absolute Transformer & Signal Recovery 🛰️💎
        // The API 'Title' maps to Domain 'Intent'
        // The API 'Description' maps to Domain 'Reasoning'
        // The API Metadata 'Description' maps to Domain 'ExecutiveSummary'
        return new ProgressActivity(
            dto.Metadata.Name,
            dto.Metadata.Id, 
            DateTimeOffset.Parse(dto.Metadata.CreateTime, CultureInfo.InvariantCulture), 
            ActivityOriginatorMapper.Map(dto.Metadata.Originator),
            payload.Title ?? string.Empty, // Intent
            payload.Description, // Reasoning
            ArtifactMappingHelper.MapArtifacts(dto.Metadata.Artifacts),
            dto.Metadata.Description); // Executive Summary 👸✨
    }
}
