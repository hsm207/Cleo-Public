#pragma warning disable CA1819 // Properties should not return arrays (Allowed for DTOs)

using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

/// <summary>
/// Response message for the activities.list RPC.
/// </summary>
public sealed record JulesListActivitiesResponseDto(
    [property: JsonPropertyName("activities")] JulesActivityDto[]? Activities,
    [property: JsonPropertyName("nextPageToken")] string? NextPageToken
);
