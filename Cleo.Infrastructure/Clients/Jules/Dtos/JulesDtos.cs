using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos;

#pragma warning disable CA1819 // Properties should not return arrays (Allowed for DTOs)

public sealed record ListActivitiesResponse(
    [property: JsonPropertyName("activities")] JulesActivityDto[]? Activities,
    [property: JsonPropertyName("nextPageToken")] string? NextPageToken
);

public sealed record ListSourcesResponse(
    [property: JsonPropertyName("sources")] JulesSourceDto[]? Sources,
    [property: JsonPropertyName("nextPageToken")] string? NextPageToken
);

public sealed record JulesSourceDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("githubRepo")] GithubRepoDto? GithubRepo
);

public sealed record GithubRepoDto(
    [property: JsonPropertyName("owner")] string Owner,
    [property: JsonPropertyName("repo")] string Repo
);

public sealed record JulesActivityDto(
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

public sealed record PlanGeneratedDto(
    [property: JsonPropertyName("plan")] PlanDto Plan
);

public sealed record PlanDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("steps")] PlanStepDto[] Steps
);

public sealed record PlanStepDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("index")] int Index
);

public sealed record PlanApprovedDto(
    [property: JsonPropertyName("planId")] string PlanId
);

public sealed record ArtifactDto(
    [property: JsonPropertyName("changeSet")] ChangeSetDto? ChangeSet
);

public sealed record ChangeSetDto(
    [property: JsonPropertyName("gitPatch")] GitPatchDto? GitPatch
);

public sealed record GitPatchDto(
    [property: JsonPropertyName("unidiffPatch")] string UnidiffPatch,
    [property: JsonPropertyName("baseCommitId")] string BaseCommitId
);

public sealed record SessionFailedDto(
    [property: JsonPropertyName("reason")] string Reason
);

public sealed record JulesSessionDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("prompt")] string Prompt,
    [property: JsonPropertyName("sourceContext")] SourceContextDto SourceContext,
    [property: JsonPropertyName("url")] Uri? Url
);

public sealed record SourceContextDto(
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("githubRepoContext")] GithubRepoContextDto? GithubRepoContext
);

public sealed record GithubRepoContextDto(
    [property: JsonPropertyName("startingBranch")] string StartingBranch
);
