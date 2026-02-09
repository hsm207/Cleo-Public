#pragma warning disable CA1819 // Properties should not return arrays (Allowed for DTOs)

using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

public sealed record BashOutputDto(
    [property: JsonPropertyName("command")] string Command,
    [property: JsonPropertyName("output")] string Output,
    [property: JsonPropertyName("exitCode")] int ExitCode
);
