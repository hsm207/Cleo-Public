using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

public sealed record JulesUnknownPayloadDto(
    string RawType,
    string? RawJson
) : JulesActivityPayloadDto;
