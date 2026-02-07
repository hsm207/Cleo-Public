using Cleo.Core.Domain.Common;
using Cleo.Infrastructure.Messaging;
using MediatR;
using Moq;
using Xunit;

namespace Cleo.Infrastructure.Tests.Messaging;

public sealed class MediatRDispatcherTests
{
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly MediatRDispatcher _sut;

    public MediatRDispatcherTests()
    {
        _sut = new MediatRDispatcher(_mockMediator.Object);
    }

    [Fact(DisplayName = "MediatRDispatcher should wrap domain events in an envelope and publish to mediator.")]
    public async Task DispatchAsync_Single_ShouldPublishToMediator()
    {
        // Arrange
        var @event = new TestDomainEvent(DateTimeOffset.UtcNow);
        var ct = TestContext.Current.CancellationToken;

        // Act
        await _sut.DispatchAsync(@event, ct);

        // Assert
        _mockMediator.Verify(m => m.Publish(
            It.Is<INotification>(n => n is DomainEventNotification<TestDomainEvent> && ((DomainEventNotification<TestDomainEvent>)n).Event == @event), 
            ct), Times.Once);
    }

    [Fact(DisplayName = "MediatRDispatcher should publish collections of wrapped events.")]
    public async Task DispatchAsync_Collection_ShouldPublishAllToMediator()
    {
        // Arrange
        var events = new[] { new TestDomainEvent(DateTimeOffset.UtcNow), new TestDomainEvent(DateTimeOffset.UtcNow) };
        var ct = TestContext.Current.CancellationToken;

        // Act
        await _sut.DispatchAsync(events, ct);

        // Assert
        foreach (var @event in events)
        {
            _mockMediator.Verify(m => m.Publish(
                It.Is<INotification>(n => n is DomainEventNotification<TestDomainEvent> && ((DomainEventNotification<TestDomainEvent>)n).Event == @event), 
                ct), Times.Once);
        }
    }

    [Fact(DisplayName = "MediatRDispatcher should validate arguments.")]
    public async Task DispatchAsync_ShouldThrowOnNull()
    {
        var ct = TestContext.Current.CancellationToken;
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.DispatchAsync((IDomainEvent)null!, ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.DispatchAsync((IEnumerable<IDomainEvent>)null!, ct));
    }

    [Fact(DisplayName = "MediatRDispatcher should throw if mediator is null.")]
    public void Constructor_ShouldThrowOnNullMediator()
    {
        Assert.Throws<ArgumentNullException>(() => new MediatRDispatcher(null!));
    }

    // REAL VIBE: Concrete event record
    private sealed record TestDomainEvent(DateTimeOffset OccurredOn) : IDomainEvent;
}
