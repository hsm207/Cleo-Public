#pragma warning disable CA1819 // Properties should not return arrays (Allowed for DTOs)

using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

/// <summary>
/// Data transfer object representing a Jules engineering session.
/// </summary>
public sealed record JulesSessionResponse(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("state")] JulesSessionState State,
    [property: JsonPropertyName("prompt")] string Prompt,
    [property: JsonPropertyName("sourceContext")] SourceContextDto SourceContext,
    [property: JsonPropertyName("url")] Uri? Url,
    [property: JsonPropertyName("requirePlanApproval")] bool? RequirePlanApproval,
    [property: JsonPropertyName("automationMode")] JulesAutomationMode? AutomationMode,
    [property: JsonPropertyName("createTime")] DateTimeOffset? CreateTime,
    [property: JsonPropertyName("updateTime")] DateTimeOffset? UpdateTime,
    [property: JsonPropertyName("outputs")] JulesOutputDto[]? Outputs = null
);

public sealed record JulesOutputDto(
    [property: JsonPropertyName("changeSet")] ChangeSetDto? ChangeSet,
    [property: JsonPropertyName("pullRequest")] PullRequestDto? PullRequest
);

public sealed record PullRequestDto(
    [property: JsonPropertyName("url")] Uri Url,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string? Description
);
