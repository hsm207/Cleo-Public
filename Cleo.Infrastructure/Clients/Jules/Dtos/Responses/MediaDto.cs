using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

/// <summary>
/// Represents a media artifact in the Jules API.
/// </summary>
public sealed record MediaDto(
    [property: JsonPropertyName("data")] string? Data,
    [property: JsonPropertyName("mimeType")] string? MimeType
)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}
