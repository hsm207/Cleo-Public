using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

public sealed record JulesGitPatchDto(
    [property: JsonPropertyName("unidiffPatch")] string? UnidiffPatch,
    [property: JsonPropertyName("baseCommitId")] string? BaseCommitId,
    [property: JsonPropertyName("suggestedCommitMessage")] string? SuggestedCommitMessage
)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}
