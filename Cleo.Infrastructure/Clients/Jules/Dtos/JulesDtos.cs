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
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("createTime")] DateTimeOffset CreateTime,
    [property: JsonPropertyName("originator")] string Originator,
    [property: JsonPropertyName("artifacts")] ArtifactDto[]? Artifacts,
    [property: JsonPropertyName("planGenerated")] PlanGeneratedDto? PlanGenerated,
    [property: JsonPropertyName("planApproved")] PlanApprovedDto? PlanApproved,
    [property: JsonPropertyName("userMessaged")] UserMessagedDto? UserMessaged,
    [property: JsonPropertyName("agentMessaged")] AgentMessagedDto? AgentMessaged,
    [property: JsonPropertyName("progressUpdated")] ProgressUpdatedDto? ProgressUpdated,
    [property: JsonPropertyName("sessionCompleted")] SessionCompletedDto? SessionCompleted,
    [property: JsonPropertyName("sessionFailed")] SessionFailedDto? SessionFailed
);

public sealed record UserMessagedDto(
    [property: JsonPropertyName("userMessage")] string UserMessage
);

public sealed record AgentMessagedDto(
    [property: JsonPropertyName("agentMessage")] string AgentMessage
);

public sealed record PlanGeneratedDto(
    [property: JsonPropertyName("plan")] PlanDto Plan
);

public sealed record PlanDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("steps")] PlanStepDto[] Steps,
    [property: JsonPropertyName("createTime")] DateTimeOffset CreateTime
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

public sealed record ProgressUpdatedDto(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string? Description
);

public sealed record SessionCompletedDto();

public sealed record ArtifactDto(
    [property: JsonPropertyName("changeSet")] ChangeSetDto? ChangeSet,
    [property: JsonPropertyName("media")] MediaDto? Media,
    [property: JsonPropertyName("bashOutput")] BashOutputDto? BashOutput
);

public sealed record MediaDto(
    [property: JsonPropertyName("data")] string Data,
    [property: JsonPropertyName("mimeType")] string MimeType
);

public sealed record BashOutputDto(
    [property: JsonPropertyName("command")] string Command,
    [property: JsonPropertyName("output")] string Output,
    [property: JsonPropertyName("exitCode")] int ExitCode
);

public sealed record ChangeSetDto(
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("gitPatch")] GitPatchDto? GitPatch
);

public sealed record GitPatchDto(
    [property: JsonPropertyName("unidiffPatch")] string UnidiffPatch,
    [property: JsonPropertyName("baseCommitId")] string BaseCommitId,
    [property: JsonPropertyName("suggestedCommitMessage")] string? SuggestedCommitMessage
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
    [property: JsonPropertyName("url")] Uri? Url,
    [property: JsonPropertyName("requirePlanApproval")] bool? RequirePlanApproval,
    [property: JsonPropertyName("automationMode")] string? AutomationMode,
    [property: JsonPropertyName("createTime")] DateTimeOffset? CreateTime,
    [property: JsonPropertyName("updateTime")] DateTimeOffset? UpdateTime
);

public sealed record SourceContextDto(
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("githubRepoContext")] GithubRepoContextDto? GithubRepoContext
);

public sealed record GithubRepoContextDto(
    [property: JsonPropertyName("startingBranch")] string StartingBranch
);
