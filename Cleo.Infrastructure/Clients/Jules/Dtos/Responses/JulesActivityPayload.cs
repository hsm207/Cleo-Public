namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

/// <summary>
/// The specific event payload of a Jules activity, sectioned off for ACL intuition.
/// </summary>
public sealed record JulesActivityPayload(
    ProgressUpdatedDto? ProgressUpdated = null,
    PlanGeneratedDto? PlanGenerated = null,
    PlanApprovedDto? PlanApproved = null,
    UserMessagedDto? UserMessaged = null,
    AgentMessagedDto? AgentMessaged = null,
    SessionCompletedDto? SessionCompleted = null,
    SessionFailedDto? SessionFailed = null,
    ChangeSetDto? CodeChanges = null,
    BashOutputDto? BashOutput = null,
    MediaDto? Media = null
);
