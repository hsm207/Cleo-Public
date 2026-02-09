namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// Configuration options for initializing a new remote session.
/// </summary>
/// <param name="Mode">The desired automation level.</param>
/// <param name="Title">An optional human-friendly name for the session.</param>
/// <param name="RequirePlanApproval">Whether the agent must wait for explicit plan approval before starting work.</param>
public record SessionCreationOptions(
    AutomationMode Mode = AutomationMode.None,
    string? Title = null,
    bool RequirePlanApproval = true
);
