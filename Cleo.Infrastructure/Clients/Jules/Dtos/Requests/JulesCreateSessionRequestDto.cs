using System.Text.Json.Serialization;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Dtos.Requests;

/// <summary>
/// Data transfer object for initiating a new engineering session.
/// </summary>
internal sealed record JulesCreateSessionRequestDto(
    [property: JsonPropertyName("prompt")] string Prompt,
    [property: JsonPropertyName("sourceContext")] JulesSourceContextDto SourceContext,
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("requirePlanApproval")] bool RequirePlanApproval = true,
    [property: JsonPropertyName("automationMode")] string AutomationMode = "AUTO_CREATE_PR"
);
