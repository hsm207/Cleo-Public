using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

/// <summary>
/// Represents the possible states of a Jules session as defined in the API discovery doc.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JulesSessionState
{
    [JsonPropertyName("STATE_UNSPECIFIED")]
    StateUnspecified,
    [JsonPropertyName("QUEUED")]
    Queued,
    [JsonPropertyName("PLANNING")]
    Planning, // Deprecated but still in discovery
    [JsonPropertyName("AWAITING_PLAN_APPROVAL")]
    AwaitingPlanApproval,
    [JsonPropertyName("AWAITING_USER_FEEDBACK")]
    AwaitingUserFeedback,
    [JsonPropertyName("IN_PROGRESS")]
    InProgress,
    [JsonPropertyName("PAUSED")]
    Paused,
    [JsonPropertyName("FAILED")]
    Failed,
    [JsonPropertyName("COMPLETED")]
    Completed
}
