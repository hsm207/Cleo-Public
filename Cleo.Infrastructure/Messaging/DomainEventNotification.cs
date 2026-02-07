using Cleo.Core.Domain.Common;
using MediatR;

namespace Cleo.Infrastructure.Messaging;

/// <summary>
/// A MediatR-specific envelope that wraps a pure domain event for in-process dispatching.
/// This isolates the library dependency to the infrastructure layer.
/// </summary>
/// <typeparam name="TEvent">The type of the domain event.</typeparam>
internal sealed record DomainEventNotification<TEvent>(TEvent Event) : INotification 
    where TEvent : IDomainEvent;
