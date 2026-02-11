using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

public sealed record PlanGeneratedDto(
    [property: JsonPropertyName("plan")] PlanDto Plan
)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}
