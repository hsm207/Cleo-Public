using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

public sealed record ArtifactDto(
    [property: JsonPropertyName("changeSet")] ChangeSetDto? ChangeSet,
    [property: JsonPropertyName("media")] MediaDto? Media,
    [property: JsonPropertyName("bashOutput")] BashOutputDto? BashOutput
)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}
