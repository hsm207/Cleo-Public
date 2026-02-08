using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// A fallback mapper that handles unrecognized Jules API activities safely.
/// </summary>
internal sealed class UnknownActivityMapper : IJulesActivityMapper
{
    public bool CanMap(JulesActivityDto dto) => true;

    public SessionActivity Map(JulesActivityDto dto)
    {
        return new MessageActivity(dto.Id, dto.CreateTime, ActivityOriginator.System, $"Unknown activity type received: {dto.Description}");
    }
}
