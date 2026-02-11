using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

public sealed record ChangeSetDto(
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("gitPatch")] GitPatchDto? GitPatch
)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}
