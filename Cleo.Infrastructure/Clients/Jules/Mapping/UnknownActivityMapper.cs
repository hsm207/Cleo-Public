using System.Globalization;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// A fallback mapper that handles unrecognized Jules API activities safely.
/// </summary>
internal sealed class UnknownActivityMapper : IJulesActivityMapper<JulesUnknownPayloadDto>
{
    public SessionActivity Map(JulesActivityDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var payload = (JulesUnknownPayloadDto)dto.Payload;

        return new ProgressActivity(
            dto.Metadata.Name,
            dto.Metadata.Id,
            DateTimeOffset.Parse(dto.Metadata.CreateTime, CultureInfo.InvariantCulture),
            ActivityOriginator.System,
            $"Unknown Activity Type: {payload.RawType}", // Intent
            "Raw JSON preserved in logs.", // Reasoning
            ArtifactMappingHelper.MapArtifacts(dto.Metadata.Artifacts),
            dto.Metadata.Description); // RFC 016: Executive Summary
    }
}
