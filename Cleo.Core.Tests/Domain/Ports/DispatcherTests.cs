using Cleo.Core.Domain.Common;
using Cleo.Core.Domain.Ports;
using Moq;
using Xunit;

namespace Cleo.Core.Tests.Domain.Ports;

public class DispatcherTests
{
    [Fact(DisplayName = "The IDispatcher port should define a clear contract for broadcasting single and multiple domain events.")]
    public async Task DispatcherPortShouldDefineStandardOperations()
    {
        var mockDispatcher = new Mock<IDispatcher>();
        var mockEvent = new Mock<IDomainEvent>();
        var events = new List<IDomainEvent> { mockEvent.Object };

        // Verify Single Dispatch
        await mockDispatcher.Object.DispatchAsync(mockEvent.Object);
        mockDispatcher.Verify(d => d.DispatchAsync(mockEvent.Object, It.IsAny<CancellationToken>()), Times.Once);

        // Verify Collection Dispatch
        await mockDispatcher.Object.DispatchAsync(events);
        mockDispatcher.Verify(d => d.DispatchAsync(events, It.IsAny<CancellationToken>()), Times.Once);
    }
}
