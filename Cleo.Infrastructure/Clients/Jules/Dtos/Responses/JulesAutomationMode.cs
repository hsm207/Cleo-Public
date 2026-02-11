using System.Text.Json.Serialization;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

/// <summary>
/// Represents the automation modes for a session as defined in the API discovery doc.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JulesAutomationMode
{
    [JsonStringEnumMemberName("AUTOMATION_MODE_UNSPECIFIED")]
    AutomationModeUnspecified,
    [JsonStringEnumMemberName("AUTO_CREATE_PR")]
    AutoCreatePr
}
