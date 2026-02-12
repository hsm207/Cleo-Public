using Cleo.Core.Domain.Common;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// Defines the contract for broadcasting domain events to subscribers.
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Dispatches a single domain event to all interested handlers.
    /// </summary>
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
