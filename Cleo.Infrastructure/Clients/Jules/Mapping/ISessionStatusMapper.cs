using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

/// <summary>
/// Defines a contract for mapping raw Jules API state strings into domain-centric SessionStatus value objects.
/// </summary>
public interface ISessionStatusMapper
{
    SessionStatus Map(string? state);
}
