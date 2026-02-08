using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps various Jules API messaging activities (user, agent, plan approval) into domain-centric MessageActivity objects.
/// </summary>
internal sealed class MessageActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => 
        dto.UserMessaged is not null || 
        dto.AgentMessaged is not null || 
        dto.PlanApproved is not null || 
        string.Equals(dto.Originator, "USER", StringComparison.OrdinalIgnoreCase);

    public SessionActivity Map(JulesActivityDto dto)
    {
        var originator = dto.Originator switch {
            _ when string.Equals(dto.Originator, "USER", StringComparison.OrdinalIgnoreCase) => ActivityOriginator.User,
            _ when string.Equals(dto.Originator, "AGENT", StringComparison.OrdinalIgnoreCase) => ActivityOriginator.Agent,
            _ => ActivityOriginator.System
        };

        var text = dto.UserMessaged?.UserMessage 
            ?? dto.AgentMessaged?.AgentMessage 
            ?? (dto.PlanApproved is not null ? $"Plan {dto.PlanApproved.PlanId} approved." : "Unknown activity.");

        return new MessageActivity(dto.Id, dto.CreateTime, originator, text);
    }
}
