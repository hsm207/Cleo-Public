using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// A high-signal mapper using modern C# patterns to hydrate domain activities from Jules DTOs.
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

    public static SessionActivity Map(JulesActivityDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return dto switch
        {
            // 1. Planning Activity 🗺️
            { PlanGenerated: not null } => new PlanningActivity(
                dto.Id, 
                dto.CreateTime, 
                dto.PlanGenerated.Plan.Steps.Select(s => new PlanStep(s.Index, s.Title, s.Description ?? string.Empty)).ToList()),

            // 2. Result Activity (Patch) 🏹
            { Artifacts: not null } when dto.Artifacts.FirstOrDefault(a => a.ChangeSet?.GitPatch != null) is { } art => new ResultActivity(
                dto.Id, 
                dto.CreateTime, 
                new SolutionPatch(art.ChangeSet!.GitPatch!.UnidiffPatch, art.ChangeSet.GitPatch.BaseCommitId)),

            // 3. Progress Activity 💓
            { ProgressUpdated: not null } => new ProgressActivity(dto.Id, dto.CreateTime, "Activity update received."),

            // 4. Failure Activity 🛑
            { SessionFailed: not null } => new FailureActivity(dto.Id, dto.CreateTime, dto.SessionFailed.Reason),

            // 5. Message Activity (Dialogue/Feedback) 💬
            _ when dto.MessageText != null || dto.PlanApproved != null || string.Equals(dto.Originator, "USER", StringComparison.OrdinalIgnoreCase) 
                => MapMessage(dto),

            _ => throw new InvalidOperationException($"No suitable mapping pattern found for activity {dto.Id}.")
        };
    }

    private static MessageActivity MapMessage(JulesActivityDto dto)
    {
        var originator = dto.Originator switch
        {
            _ when string.Equals(dto.Originator, "USER", StringComparison.OrdinalIgnoreCase) => ActivityOriginator.User,
            _ when string.Equals(dto.Originator, "AGENT", StringComparison.OrdinalIgnoreCase) => ActivityOriginator.Agent,
            _ => ActivityOriginator.System
        };

        var text = dto.MessageText ?? (dto.PlanApproved != null ? $"Plan {dto.PlanApproved.PlanId} approved." : "Unknown activity.");
        return new MessageActivity(dto.Id, dto.CreateTime, originator, text);
    }
}
