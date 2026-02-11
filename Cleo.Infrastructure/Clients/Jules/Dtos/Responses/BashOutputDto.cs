using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

public sealed record BashOutputDto(
    [property: JsonPropertyName("command")] string? Command,
    [property: JsonPropertyName("output")] string? Output,
    [property: JsonPropertyName("exitCode")] int? ExitCode
)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}
