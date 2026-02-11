using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Tests.Jules;

internal static class JulesDtoTestFactory
{
    public static JulesActivityDto Create(
        string name,
        string id,
        string? description,
        string createTime,
        string originator,
        List<ArtifactDto>? artifacts = null,
        PlanGeneratedDto? planGenerated = null,
        PlanApprovedDto? planApproved = null,
        UserMessagedDto? userMessaged = null,
        AgentMessagedDto? agentMessaged = null,
        ProgressUpdatedDto? progressUpdated = null,
        SessionCompletedDto? sessionCompleted = null,
        SessionFailedDto? sessionFailed = null)
    {
        var metadata = new JulesActivityMetadata(id, name, description, createTime, originator, artifacts);
        
        JulesActivityPayload payload = null!;

        if (progressUpdated != null) payload = new ProgressUpdatedPayload(progressUpdated.Title, progressUpdated.Description);
        else if (planGenerated != null) payload = new PlanGeneratedPayload(planGenerated.Plan);
        else if (planApproved != null) payload = new PlanApprovedPayload(planApproved.PlanId);
        else if (userMessaged != null) payload = new UserMessagedPayload(userMessaged.UserMessage);
        else if (agentMessaged != null) payload = new AgentMessagedPayload(agentMessaged.AgentMessage);
        else if (sessionCompleted != null) payload = new SessionCompletedPayload();
        else if (sessionFailed != null) payload = new SessionFailedPayload(sessionFailed.Reason);
        else payload = new ProgressUpdatedPayload("Fallback", "Synthetic test payload");

        return new JulesActivityDto(metadata, payload);
    }
}
