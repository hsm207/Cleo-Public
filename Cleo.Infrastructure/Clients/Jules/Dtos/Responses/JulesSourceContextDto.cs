#pragma warning disable CA1819 // Properties should not return arrays (Allowed for DTOs)

using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

public sealed record JulesSourceContextDto(
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("githubRepoContext")] JulesGithubRepoContextDto? GithubRepoContext,
    [property: JsonPropertyName("environmentVariablesEnabled")] bool? EnvironmentVariablesEnabled = null
);
