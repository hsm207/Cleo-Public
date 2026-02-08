#pragma warning disable CA1819 // Properties should not return arrays (Allowed for DTOs)

using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

public sealed record ProgressUpdatedDto(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string? Description
);
