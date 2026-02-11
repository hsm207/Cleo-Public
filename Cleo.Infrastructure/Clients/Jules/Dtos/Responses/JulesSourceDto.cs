#pragma warning disable CA1819 // Properties should not return arrays (Allowed for DTOs)

using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

public sealed record JulesSourceDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("githubRepo")] JulesGithubRepoDto? GithubRepo
);
