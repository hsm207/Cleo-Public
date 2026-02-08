#pragma warning disable CA1819 // Properties should not return arrays (Allowed for DTOs)

using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

public sealed record GitPatchDto(
    [property: JsonPropertyName("unidiffPatch")] string UnidiffPatch,
    [property: JsonPropertyName("baseCommitId")] string BaseCommitId,
    [property: JsonPropertyName("suggestedCommitMessage")] string? SuggestedCommitMessage
);
