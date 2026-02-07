using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos;

internal sealed record ListActivitiesResponse(
    [property: JsonPropertyName("activities")] JulesActivityDto[]? Activities,
    [property: JsonPropertyName("nextPageToken")] string? NextPageToken
);

internal sealed record JulesActivityDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("createTime")] DateTimeOffset CreateTime,
    [property: JsonPropertyName("originator")] string Originator,
    [property: JsonPropertyName("planGenerated")] PlanGeneratedDto? PlanGenerated,
    [property: JsonPropertyName("planApproved")] PlanApprovedDto? PlanApproved,
    [property: JsonPropertyName("messageText")] string? MessageText,
    [property: JsonPropertyName("artifacts")] ArtifactDto[]? Artifacts,
    [property: JsonPropertyName("progressUpdated")] object? ProgressUpdated,
    [property: JsonPropertyName("sessionFailed")] SessionFailedDto? SessionFailed
);

internal sealed record PlanGeneratedDto(
    [property: JsonPropertyName("plan")] PlanDto Plan
);

internal sealed record PlanDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("steps")] PlanStepDto[] Steps
);

internal sealed record PlanStepDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("index")] int Index
);

internal sealed record PlanApprovedDto(
    [property: JsonPropertyName("planId")] string PlanId
);

internal sealed record ArtifactDto(
    [property: JsonPropertyName("changeSet")] ChangeSetDto? ChangeSet
);

internal sealed record ChangeSetDto(
    [property: JsonPropertyName("gitPatch")] GitPatchDto? GitPatch
);

internal sealed record GitPatchDto(
    [property: JsonPropertyName("unidiffPatch")] string UnidiffPatch,
    [property: JsonPropertyName("baseCommitId")] string BaseCommitId
);

internal sealed record SessionFailedDto(
    [property: JsonPropertyName("reason")] string Reason
);

internal sealed record JulesSessionDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("prompt")] string Prompt,
    [property: JsonPropertyName("sourceContext")] SourceContextDto SourceContext
);

internal sealed record SourceContextDto(
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("githubRepoContext")] GithubRepoContextDto? GithubRepoContext
);

internal sealed record GithubRepoContextDto(
    [property: JsonPropertyName("startingBranch")] string StartingBranch
);
