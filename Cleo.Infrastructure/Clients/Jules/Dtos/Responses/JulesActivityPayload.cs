using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

/// <summary>
/// The specific event payload of a Jules activity.
/// This hierarchy uses polymorphism to handle the 'OneOf' structure of the API.
/// </summary>
public abstract record JulesActivityPayload
{
    // Captures any unknown properties within the payload object for perfect fidelity 🛡️💎
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}

public sealed record ProgressUpdatedPayload(
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("description")] string? Description
) : JulesActivityPayload;

public sealed record PlanGeneratedPayload(
    [property: JsonPropertyName("plan")] PlanDto Plan
) : JulesActivityPayload;

public sealed record PlanApprovedPayload(
    [property: JsonPropertyName("planId")] string? PlanId
) : JulesActivityPayload;

public sealed record UserMessagedPayload(
    [property: JsonPropertyName("userMessage")] string? UserMessage
) : JulesActivityPayload;

public sealed record AgentMessagedPayload(
    [property: JsonPropertyName("agentMessage")] string? AgentMessage
) : JulesActivityPayload;

public sealed record SessionCompletedPayload : JulesActivityPayload;

public sealed record SessionFailedPayload(
    [property: JsonPropertyName("reason")] string? Reason
) : JulesActivityPayload;

public sealed record CodeChangesPayload(
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("gitPatch")] GitPatchDto? GitPatch
) : JulesActivityPayload;

public sealed record BashOutputPayload(
    [property: JsonPropertyName("command")] string? Command,
    [property: JsonPropertyName("output")] string? Output,
    [property: JsonPropertyName("exitCode")] int? ExitCode
) : JulesActivityPayload;

public sealed record MediaPayload(
    [property: JsonPropertyName("data")] string? Data,
    [property: JsonPropertyName("mimeType")] string? MimeType
) : JulesActivityPayload;
