using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Defines a contract for mapping raw Jules API activity DTOs into domain-centric SessionActivity objects.
/// </summary>
public interface IJulesActivityMapper
{
    SessionActivity Map(JulesActivityDto dto);
}

/// <summary>
/// Strongly-typed contract for mapping specific payload types.
/// </summary>
public interface IJulesActivityMapper<TPayload> : IJulesActivityMapper
    where TPayload : JulesActivityPayloadDto
{
}
