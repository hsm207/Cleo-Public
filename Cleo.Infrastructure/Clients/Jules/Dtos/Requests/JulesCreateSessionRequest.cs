using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Requests;

/// <summary>
/// Data transfer object for initiating a new engineering session.
/// </summary>
internal sealed record JulesCreateSessionRequest(
    [property: JsonPropertyName("prompt")] string Prompt,
    [property: JsonPropertyName("sourceContext")] JulesSourceContextDto SourceContext,
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("requirePlanApproval")] bool RequirePlanApproval = true,
    [property: JsonPropertyName("automationMode")] string AutomationMode = "AUTO_CREATE_PR"
);

internal sealed record JulesSourceContextDto(
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("githubRepoContext")] JulesGithubRepoContextDto? GithubRepoContext = null
);

internal sealed record JulesGithubRepoContextDto(
    [property: JsonPropertyName("startingBranch")] string StartingBranch
);
