using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// A high-signal mapper for static Jules DTO mapping.
/// </summary>
internal static class JulesMapper
{
    public static Session Map(JulesSessionDto dto, TaskDescription originalTask)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Session(
            new SessionId(dto.Name),
            originalTask,
            new SourceContext(dto.SourceContext.Source, dto.SourceContext.GithubRepoContext?.StartingBranch ?? string.Empty),
            new SessionPulse(MapStatus(dto.State), $"Session is {dto.State}")
        );
    }

    public static SessionStatus MapStatus(string state) => state.ToUpperInvariant() switch
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
