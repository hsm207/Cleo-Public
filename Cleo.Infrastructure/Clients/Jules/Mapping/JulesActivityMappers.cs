using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

internal interface IJulesActivityMapper
{
    bool CanMap(JulesActivityDto dto);
    SessionActivity Map(JulesActivityDto dto);
}

internal interface ISessionStatusMapper
{
    SessionStatus Map(string state);
}

internal sealed class DefaultSessionStatusMapper : ISessionStatusMapper
{
    private static readonly Dictionary<string, SessionStatus> StatusMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["STARTING_UP"] = SessionStatus.StartingUp,
        ["PLANNING"] = SessionStatus.Planning,
        ["IN_PROGRESS"] = SessionStatus.InProgress,
        ["AWAITING_FEEDBACK"] = SessionStatus.AwaitingFeedback,
        ["COMPLETED"] = SessionStatus.Completed,
        ["FAILED"] = SessionStatus.Failed
    };

    public SessionStatus Map(string state) => 
        StatusMap.TryGetValue(state, out var status) ? status : SessionStatus.InProgress;
}

internal sealed class PlanningActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.PlanGenerated is not null;
    public SessionActivity Map(JulesActivityDto dto) => new PlanningActivity(
        dto.Id, dto.CreateTime, 
        dto.PlanGenerated!.Plan.Steps.Select(s => new PlanStep(s.Index, s.Title, s.Description ?? string.Empty)).ToList());
}

internal sealed class ResultActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.Artifacts?.Any(a => a.ChangeSet?.GitPatch is not null) ?? false;
    public SessionActivity Map(JulesActivityDto dto)
    {
        var art = dto.Artifacts!.First(a => a.ChangeSet?.GitPatch is not null);
        var patchDto = art.ChangeSet!.GitPatch!;
        return new ResultActivity(dto.Id, dto.CreateTime, new SolutionPatch(patchDto.UnidiffPatch, patchDto.BaseCommitId));
    }
}

internal sealed class ProgressActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.ProgressUpdated is not null;
    public SessionActivity Map(JulesActivityDto dto) => new ProgressActivity(dto.Id, dto.CreateTime, "Activity update received.");
}

internal sealed class FailureActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.SessionFailed is not null;
    public SessionActivity Map(JulesActivityDto dto) => new FailureActivity(dto.Id, dto.CreateTime, dto.SessionFailed!.Reason);
}

internal sealed class MessageActivityMapper : IJulesActivityMapper
{
    // The "Fallback" strategy - handles messages and unknowns safely. 🧤
    public bool CanMap(JulesActivityDto dto) => dto.MessageText is not null || dto.PlanApproved is not null || string.Equals(dto.Originator, "USER", StringComparison.OrdinalIgnoreCase);
    public SessionActivity Map(JulesActivityDto dto)
    {
        var originator = dto.Originator switch {
            _ when string.Equals(dto.Originator, "USER", StringComparison.OrdinalIgnoreCase) => ActivityOriginator.User,
            _ when string.Equals(dto.Originator, "AGENT", StringComparison.OrdinalIgnoreCase) => ActivityOriginator.Agent,
            _ => ActivityOriginator.System
        };
        var text = dto.MessageText ?? (dto.PlanApproved is not null ? $"Plan {dto.PlanApproved.PlanId} approved." : "Unknown activity.");
        return new MessageActivity(dto.Id, dto.CreateTime, originator, text);
    }
}
