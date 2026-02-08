using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Defines a contract for mapping raw Jules API activity DTOs into domain-centric SessionActivity objects.
/// </summary>
public interface IJulesActivityMapper
{
    bool CanMap(JulesActivityDto dto);
    SessionActivity Map(JulesActivityDto dto);
}
