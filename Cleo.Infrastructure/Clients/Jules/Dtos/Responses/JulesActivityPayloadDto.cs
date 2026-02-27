using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

/// <summary>
/// The specific event payload of a Jules activity.
/// This hierarchy uses polymorphism to handle the 'OneOf' structure of the API.
/// </summary>
public abstract record JulesActivityPayloadDto
{
    // Captures any unknown properties within the payload object for perfect fidelity 🛡️💎
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}

public sealed record JulesProgressUpdatedPayloadDto(
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("description")] string? Description
) : JulesActivityPayloadDto;

public sealed record JulesPlanGeneratedPayloadDto(
    [property: JsonPropertyName("plan")] JulesPlanDto Plan
) : JulesActivityPayloadDto;

public sealed record JulesPlanApprovedPayloadDto(
    [property: JsonPropertyName("planId")] string? PlanId
) : JulesActivityPayloadDto;

public sealed record JulesUserMessagedPayloadDto(
    [property: JsonPropertyName("userMessage")] string? UserMessage
) : JulesActivityPayloadDto;

public sealed record JulesAgentMessagedPayloadDto(
    [property: JsonPropertyName("agentMessage")] string? AgentMessage
) : JulesActivityPayloadDto;

public sealed record JulesSessionCompletedPayloadDto : JulesActivityPayloadDto;

public sealed record JulesSessionFailedPayloadDto(
    [property: JsonPropertyName("reason")] string? Reason
) : JulesActivityPayloadDto;

public sealed record JulesCodeChangesPayloadDto(
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("gitPatch")] JulesGitPatchDto? GitPatch
) : JulesActivityPayloadDto;

public sealed record JulesBashOutputPayloadDto(
    [property: JsonPropertyName("command")] string? Command,
    [property: JsonPropertyName("output")] string? Output,
    [property: JsonPropertyName("exitCode")] int? ExitCode
) : JulesActivityPayloadDto;

public sealed record JulesMediaPayloadDto(
    [property: JsonPropertyName("data")] string? Data,
    [property: JsonPropertyName("mimeType")] string? MimeType
) : JulesActivityPayloadDto;

public sealed record JulesArtifactsPayloadDto(
    [property: JsonPropertyName("artifacts")] IReadOnlyList<JulesArtifactDto>? Artifacts
) : JulesActivityPayloadDto;
