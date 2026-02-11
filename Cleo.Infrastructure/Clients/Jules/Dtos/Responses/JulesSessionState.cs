using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

/// <summary>
/// Represents the possible states of a Jules session as defined in the API discovery doc.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JulesSessionState
{
    [JsonStringEnumMemberName("STATE_UNSPECIFIED")]
    StateUnspecified,
    [JsonStringEnumMemberName("QUEUED")]
    Queued,
    [JsonStringEnumMemberName("PLANNING")]
    Planning, // Deprecated but still in discovery
    [JsonStringEnumMemberName("AWAITING_PLAN_APPROVAL")]
    AwaitingPlanApproval,
    [JsonStringEnumMemberName("AWAITING_USER_FEEDBACK")]
    AwaitingUserFeedback,
    [JsonStringEnumMemberName("IN_PROGRESS")]
    InProgress,
    [JsonStringEnumMemberName("PAUSED")]
    Paused,
    [JsonStringEnumMemberName("FAILED")]
    Failed,
    [JsonStringEnumMemberName("COMPLETED")]
    Completed
}
