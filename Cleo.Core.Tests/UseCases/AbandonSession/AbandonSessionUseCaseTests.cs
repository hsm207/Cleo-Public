using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.AbandonSession;
using Xunit;

namespace Cleo.Core.Tests.UseCases.AbandonSession;

public sealed class AbandonSessionUseCaseTests
{
    private readonly FakeSessionWriter _writer = new();
    private readonly AbandonSessionUseCase _sut;

    public AbandonSessionUseCaseTests()
    {
        _sut = new AbandonSessionUseCase(_writer);
    }

    [Fact(DisplayName = "When abandoning a mission, the Session should be removed from the Task Registry.")]
    public async Task ShouldAbandonSession()
    {
        // Arrange
        var sessionId = new SessionId("sessions/123");
        var request = new AbandonSessionRequest(sessionId);

        // Act
        var result = await _sut.ExecuteAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(sessionId, result.Id);
        Assert.Equal(sessionId, _writer.DeletedId);
    }

    private sealed class FakeSessionWriter : ISessionWriter
    {
        public SessionId? DeletedId { get; private set; }
        public Task SaveAsync(Cleo.Core.Domain.Entities.Session session, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(SessionId id, CancellationToken cancellationToken = default)
        {
            DeletedId = id;
            return Task.CompletedTask;
        }
    }
}
