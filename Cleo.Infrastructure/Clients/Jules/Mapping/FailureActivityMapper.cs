using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Maps Jules API 'sessionFailed' activities into domain-centric FailureActivity objects.
/// </summary>
internal sealed class FailureActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => dto.SessionFailed is not null;
    
    public SessionActivity Map(JulesActivityDto dto) => new FailureActivity(dto.Id, dto.CreateTime, dto.SessionFailed!.Reason);
}
