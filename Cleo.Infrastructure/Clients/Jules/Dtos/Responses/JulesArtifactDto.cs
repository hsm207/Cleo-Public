using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

public sealed record JulesArtifactDto(
    [property: JsonPropertyName("changeSet")] JulesChangeSetDto? ChangeSet,
    [property: JsonPropertyName("media")] JulesMediaDto? Media,
    [property: JsonPropertyName("bashOutput")] JulesBashOutputDto? BashOutput
)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}
