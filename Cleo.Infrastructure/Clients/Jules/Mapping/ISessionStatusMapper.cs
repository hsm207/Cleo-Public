using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Defines a contract for mapping raw Jules API state enums into domain-centric SessionStatus value objects.
/// </summary>
public interface ISessionStatusMapper
{
    SessionStatus Map(JulesSessionState? state);
}
