using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

public sealed record JulesChangeSetDto(
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("gitPatch")] JulesGitPatchDto? GitPatch
)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}
