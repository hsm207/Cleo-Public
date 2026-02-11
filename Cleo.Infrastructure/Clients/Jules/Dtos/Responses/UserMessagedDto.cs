using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

public sealed record UserMessagedDto(
    [property: JsonPropertyName("userMessage")] string? UserMessage
)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}
