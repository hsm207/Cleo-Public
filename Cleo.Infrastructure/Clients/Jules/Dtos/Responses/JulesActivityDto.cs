#pragma warning disable CA1819 // Properties should not return arrays (Allowed for DTOs)

using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

/// <summary>
/// Represents a single activity within a Jules session.
/// </summary>
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
