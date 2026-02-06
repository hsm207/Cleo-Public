using Cleo.Core.Domain.Common;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// A purely abstract portal for broadcasting domain events to the outside world.
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Dispatches a single domain event to all interested handlers.
    /// </summary>
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches a collection of domain events.
    /// </summary>
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
