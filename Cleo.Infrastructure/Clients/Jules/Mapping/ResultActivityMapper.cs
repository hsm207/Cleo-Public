using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps Jules API activities containing Git patches into domain-centric ResultActivity objects.
/// </summary>
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
