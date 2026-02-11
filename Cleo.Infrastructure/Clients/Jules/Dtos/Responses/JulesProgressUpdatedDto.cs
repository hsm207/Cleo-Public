using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

public sealed record JulesProgressUpdatedDto(
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("description")] string? Description
)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}
