using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps Jules API activities containing Bash terminal output into domain-centric ExecutionActivity objects.
/// </summary>
internal sealed class ExecutionActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.Artifacts?.Any(a => a.BashOutput is not null) ?? false;
    
    public SessionActivity Map(JulesActivityDto dto)
    {
        var bash = dto.Artifacts!.First(a => a.BashOutput is not null).BashOutput!;
        return new ExecutionActivity(dto.Id, dto.CreateTime, bash.Command, bash.Output, bash.ExitCode);
    }
}
