using Cleo.Core.Domain.Common;
using Cleo.Core.Domain.Ports;
using MediatR;

namespace Cleo.Infrastructure.Messaging;

/// <summary>
/// An implementation of the domain event dispatcher using the MediatR library.
/// This implementation uses an envelope to isolate the library from the core domain.
/// </summary>
public sealed class MediatRDispatcher : IDispatcher
{
    private readonly IMediator _mediator;

    public MediatRDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

#pragma warning disable CA1062 // Validate arguments of public methods (VIP Lounge Rules: We trust the caller)
    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // Wrap the domain event in an infrastructure-specific notification envelope.
        var envelopeType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        var envelope = Activator.CreateInstance(envelopeType, domainEvent);

        if (envelope is INotification notification)
        {
            await _mediator.Publish(notification, cancellationToken).ConfigureAwait(false);
        }
    }
}
