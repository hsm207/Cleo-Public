using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

/// <summary>
/// Contains the common envelope fields for any Jules activity.
/// </summary>
public sealed record JulesActivityMetadata(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("createTime")] string CreateTime,
    [property: JsonPropertyName("originator")] string Originator,
    [property: JsonPropertyName("artifacts")] IReadOnlyList<ArtifactDto>? Artifacts
)
{
    // Captures any unknown envelope-level properties for perfect fidelity 🛡️💎
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}
