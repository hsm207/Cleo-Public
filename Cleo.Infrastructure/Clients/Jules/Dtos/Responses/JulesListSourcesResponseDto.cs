#pragma warning disable CA1819 // Properties should not return arrays (Allowed for DTOs)

using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

/// <summary>
/// Response message for the sources.list RPC.
/// </summary>
public sealed record JulesListSourcesResponseDto(
    [property: JsonPropertyName("sources")] JulesSourceDto[]? Sources,
    [property: JsonPropertyName("nextPageToken")] string? NextPageToken
);
