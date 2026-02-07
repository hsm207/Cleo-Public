using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

internal interface IJulesActivityMapper
{
    bool CanMap(JulesActivityDto dto);
    SessionActivity Map(JulesActivityDto dto);
}
