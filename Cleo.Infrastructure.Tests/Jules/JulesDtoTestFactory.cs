using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Tests.Jules;

internal static class JulesDtoTestFactory
{
    public static JulesActivityDto Create(string id, string name, string? description, string createTime, string originator, IEnumerable<JulesArtifactDto>? artifacts,
        JulesProgressUpdatedPayloadDto? progressUpdated = null,
        JulesPlanGeneratedPayloadDto? planGenerated = null,
        JulesPlanApprovedPayloadDto? planApproved = null,
        JulesUserMessagedPayloadDto? userMessaged = null,
        JulesAgentMessagedPayloadDto? agentMessaged = null,
        JulesSessionCompletedPayloadDto? sessionCompleted = null,
        JulesSessionFailedPayloadDto? sessionFailed = null)
    {
        var metadata = new JulesActivityMetadataDto(id, name, description, createTime, originator, artifacts?.ToList());
        
        JulesActivityPayloadDto payload = (JulesActivityPayloadDto?)progressUpdated
            ?? (JulesActivityPayloadDto?)planGenerated
            ?? (JulesActivityPayloadDto?)planApproved
            ?? (JulesActivityPayloadDto?)userMessaged
            ?? (JulesActivityPayloadDto?)agentMessaged
            ?? (JulesActivityPayloadDto?)sessionCompleted
            ?? (JulesActivityPayloadDto?)sessionFailed
            ?? new JulesProgressUpdatedPayloadDto("Default", "Fallback");

        return new JulesActivityDto(metadata, payload);
    }
}
