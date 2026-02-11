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
        List<JulesArtifactDto>? artifacts = null,
        JulesPlanGeneratedDto? planGenerated = null,
        JulesPlanApprovedDto? planApproved = null,
        JulesUserMessagedDto? userMessaged = null,
        JulesAgentMessagedDto? agentMessaged = null,
        JulesProgressUpdatedDto? progressUpdated = null,
        JulesSessionCompletedDto? sessionCompleted = null,
        JulesSessionFailedDto? sessionFailed = null)
    {
        var metadata = new JulesActivityMetadataDto(id, name, description, createTime, originator, artifacts);
        
        JulesActivityPayloadDto payload = null!;

        if (progressUpdated != null) payload = new JulesProgressUpdatedPayloadDto(progressUpdated.Title, progressUpdated.Description);
        else if (planGenerated != null) payload = new JulesPlanGeneratedPayloadDto(planGenerated.Plan);
        else if (planApproved != null) payload = new JulesPlanApprovedPayloadDto(planApproved.PlanId);
        else if (userMessaged != null) payload = new JulesUserMessagedPayloadDto(userMessaged.UserMessage);
        else if (agentMessaged != null) payload = new JulesAgentMessagedPayloadDto(agentMessaged.AgentMessage);
        else if (sessionCompleted != null) payload = new JulesSessionCompletedPayloadDto();
        else if (sessionFailed != null) payload = new JulesSessionFailedPayloadDto(sessionFailed.Reason);
        else payload = new JulesProgressUpdatedPayloadDto("Fallback", "Synthetic test payload");

        return new JulesActivityDto(metadata, payload);
    }
}
