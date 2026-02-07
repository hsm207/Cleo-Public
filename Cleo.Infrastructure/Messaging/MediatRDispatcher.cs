using Cleo.Core.Domain.Common;
using Cleo.Core.Domain.Ports;
using MediatR;

namespace Cleo.Infrastructure.Messaging;

/// <summary>
/// An implementation of the domain event dispatcher using the MediatR library.
/// </summary>
public sealed class MediatRDispatcher : IDispatcher
{
    private readonly IMediator _mediator;

    public MediatRDispatcher(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        await _mediator.Publish(domainEvent, cancellationToken).ConfigureAwait(false);
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        foreach (var @event in domainEvents)
        {
            await DispatchAsync(@event, cancellationToken).ConfigureAwait(false);
        }
    }
}
