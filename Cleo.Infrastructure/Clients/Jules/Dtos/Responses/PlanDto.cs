using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

public sealed record PlanDto(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("steps")] IReadOnlyList<PlanStepDto>? Steps,
    [property: JsonPropertyName("createTime")] string? CreateTime
)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}
