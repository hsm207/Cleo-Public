using Cleo.Core.Domain.Common;
using Cleo.Core.Domain.Events;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Messaging;
using Cleo.Tests.Common;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Cleo.Infrastructure.Tests.Messaging;

public class MediatRIntegrationTests
{
    private readonly IServiceProvider _provider;
    private readonly SpyHandler _spy;

    public MediatRIntegrationTests()
    {
        var services = new ServiceCollection();
        _spy = new SpyHandler();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MediatRIntegrationTests).Assembly));
        // Explicitly register the handler interface mapped to the specific spy instance
        services.AddSingleton<INotificationHandler<DomainEventNotification<TestDomainEvent>>>(_spy);
        services.AddSingleton<IDispatcher, MediatRDispatcher>();

        _provider = services.BuildServiceProvider();
    }

    [Fact(DisplayName = "MediatRDispatcher should dispatch real events to handlers.")]
    public async Task ShouldDispatchEvent()
    {
        var dispatcher = _provider.GetRequiredService<IDispatcher>();
        var evt = new TestDomainEvent(TestFactory.CreateSessionId("s1"));

        await dispatcher.DispatchAsync(evt, CancellationToken.None);

        Assert.True(_spy.Handled);
        Assert.Equal(evt.SessionId, _spy.ReceivedEvent?.SessionId);
    }
}

// Define a test-specific domain event (Top-level)
public record TestDomainEvent(SessionId SessionId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

// Define a handler wrapper for the spy
internal sealed class SpyHandler : INotificationHandler<DomainEventNotification<TestDomainEvent>>
{
    public bool Handled { get; private set; }
    public TestDomainEvent? ReceivedEvent { get; private set; }

    public Task Handle(DomainEventNotification<TestDomainEvent> notification, CancellationToken cancellationToken)
    {
        Handled = true;
        ReceivedEvent = notification.Event;
        return Task.CompletedTask;
    }
}
