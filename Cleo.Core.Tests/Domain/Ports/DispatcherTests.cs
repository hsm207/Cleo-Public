using Cleo.Core.Domain.Common;
using Cleo.Core.Domain.Ports;
using Moq;
using Xunit;

namespace Cleo.Core.Tests.Domain.Ports;

public class DispatcherTests
{
    [Fact(DisplayName = "The dispatcher port should facilitate publication of domain events.")]
    public async Task DispatcherShouldPublishEvents()
    {
        // Arrange
        var mockDispatcher = new Mock<IDispatcher>();
        var @event = new TestDomainEvent(DateTimeOffset.UtcNow);
        var ct = TestContext.Current.CancellationToken;

        // Act
        await mockDispatcher.Object.DispatchAsync(@event, ct);

        // Assert
        mockDispatcher.Verify(d => d.DispatchAsync(@event, ct), Times.Once);
    }

    [Fact(DisplayName = "The dispatcher port should support bulk publication.")]
    public async Task DispatcherShouldSupportBulk()
    {
        // Arrange
        var mockDispatcher = new Mock<IDispatcher>();
        var events = new[] { new TestDomainEvent(DateTimeOffset.UtcNow) };
        var ct = TestContext.Current.CancellationToken;

        // Act
        await mockDispatcher.Object.DispatchAsync(events, ct);

        // Assert
        mockDispatcher.Verify(d => d.DispatchAsync(events, ct), Times.Once);
    }

    private sealed record TestDomainEvent(DateTimeOffset OccurredOn) : IDomainEvent;
}
