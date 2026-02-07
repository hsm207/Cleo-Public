using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos;
using Cleo.Infrastructure.Clients.Jules.Mapping;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// A S.O.L.I.D. composite mapper that hydrates domain activities from Jules DTOs.
/// </summary>
internal static class JulesMapper
{
    public static Session Map(JulesSessionDto dto, TaskDescription originalTask)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var id = new SessionId(dto.Name);
        var source = new SourceContext(
            dto.SourceContext.Source, 
            dto.SourceContext.GithubRepoContext?.StartingBranch ?? string.Empty);
        
        var status = MapStatus(dto.State);
        var pulse = new SessionPulse(status, $"Session is {dto.State}");

        return new Session(id, originalTask, source, pulse);
    }

    public static SessionStatus MapStatus(string state)
    {
        return state.ToUpperInvariant() switch
        {
            "STARTING_UP" => SessionStatus.StartingUp,
            "PLANNING" => SessionStatus.Planning,
            "IN_PROGRESS" => SessionStatus.InProgress,
            "AWAITING_FEEDBACK" => SessionStatus.AwaitingFeedback,
            "COMPLETED" => SessionStatus.Completed,
            "FAILED" => SessionStatus.Failed,
            _ => SessionStatus.InProgress
        };
    }

    public static SessionActivity Map(JulesActivityDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        // Instantiate mappers locally to ensure 100% coverage visibility 🧼✨
        var mappers = new IJulesActivityMapper[]
        {
            new PlanningActivityMapper(),
            new ResultActivityMapper(),
            new ProgressActivityMapper(),
            new FailureActivityMapper(),
            new MessageActivityMapper()
        };

        foreach (var mapper in mappers)
        {
            if (mapper.CanMap(dto))
            {
                return mapper.Map(dto);
            }
        }

        throw new InvalidOperationException($"No suitable mapper found for activity {dto.Id}.");
    }
}
