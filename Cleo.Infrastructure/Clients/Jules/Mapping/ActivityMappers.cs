using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

internal sealed class PlanningActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.PlanGenerated != null;

    public SessionActivity Map(JulesActivityDto dto)
    {
        var steps = dto.PlanGenerated!.Plan.Steps
            .Select(s => new PlanStep(s.Index, s.Title, s.Description ?? string.Empty))
            .ToList();
        return new PlanningActivity(dto.Id, dto.CreateTime, steps);
    }
}

internal sealed class ResultActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.Artifacts?.Any(a => a.ChangeSet?.GitPatch != null) ?? false;

    public SessionActivity Map(JulesActivityDto dto)
    {
        var patchArtifact = dto.Artifacts!.First(a => a.ChangeSet?.GitPatch != null);
        var patch = new SolutionPatch(
            patchArtifact.ChangeSet!.GitPatch!.UnidiffPatch,
            patchArtifact.ChangeSet.GitPatch.BaseCommitId);
        return new ResultActivity(dto.Id, dto.CreateTime, patch);
    }
}

internal sealed class ProgressActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.ProgressUpdated != null;

    public SessionActivity Map(JulesActivityDto dto)
    {
        return new ProgressActivity(dto.Id, dto.CreateTime, "Activity update received.");
    }
}

internal sealed class FailureActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.SessionFailed != null;

    public SessionActivity Map(JulesActivityDto dto)
    {
        return new FailureActivity(dto.Id, dto.CreateTime, dto.SessionFailed!.Reason);
    }
}

internal sealed class MessageActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => true; // Catch-all for messages/unknowns

    public SessionActivity Map(JulesActivityDto dto)
    {
        var originator = dto.Originator.ToUpperInvariant() switch
        {
            "USER" => ActivityOriginator.User,
            "AGENT" => ActivityOriginator.Agent,
            _ => ActivityOriginator.System
        };

        var text = dto.MessageText ?? (dto.PlanApproved != null ? $"Plan {dto.PlanApproved.PlanId} approved." : "Unknown activity.");
        return new MessageActivity(dto.Id, dto.CreateTime, originator, text);
    }
}
