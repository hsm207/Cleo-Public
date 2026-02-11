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
        var payload = new JulesActivityPayload(
            progressUpdated,
            planGenerated,
            planApproved,
            userMessaged,
            agentMessaged,
            sessionCompleted,
            sessionFailed);

        return new JulesActivityDto(metadata, payload);
    }
}
