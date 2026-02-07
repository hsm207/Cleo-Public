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

    [Fact(DisplayName = "MediatRDispatcher should publish single events to the mediator.")]
    public async Task DispatchAsync_Single_ShouldPublishToMediator()
    {
        // Arrange
        var mockEvent = new Mock<IDomainEvent>();
        var ct = TestContext.Current.CancellationToken;

        // Act
        await _sut.DispatchAsync(mockEvent.Object, ct);

        // Assert
        _mockMediator.Verify(m => m.Publish(mockEvent.Object, ct), Times.Once);
    }

    [Fact(DisplayName = "MediatRDispatcher should publish collections of events.")]
    public async Task DispatchAsync_Collection_ShouldPublishAllToMediator()
    {
        // Arrange
        var events = new[] { new Mock<IDomainEvent>().Object, new Mock<IDomainEvent>().Object };
        var ct = TestContext.Current.CancellationToken;

        // Act
        await _sut.DispatchAsync(events, ct);

        // Assert
        foreach (var @event in events)
        {
            _mockMediator.Verify(m => m.Publish(@event, ct), Times.Once);
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
}
