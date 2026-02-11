#pragma warning disable CA1819 // Properties should not return arrays (Allowed for DTOs)

using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

public sealed record JulesGithubRepoContextDto(
    [property: JsonPropertyName("startingBranch")] string StartingBranch
);
