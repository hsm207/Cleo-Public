using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.ForgetSession;
using Cleo.Tests.Common;
using Xunit;

namespace Cleo.Core.Tests.UseCases.ForgetSession;

public sealed class ForgetSessionUseCaseTests
{
    private readonly FakeSessionWriter _writer = new();
    private readonly ForgetSessionUseCase _sut;

    public ForgetSessionUseCaseTests()
    {
        _sut = new ForgetSessionUseCase(_writer);
    }

    [Fact(DisplayName = "When abandoning a session, the Session should be removed from the Task Registry.")]
    public async Task ShouldForgetSession()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("123");
        var request = new ForgetSessionRequest(sessionId);

        // Act
        var result = await _sut.ExecuteAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(sessionId, result.Id);
        Assert.Equal(sessionId, _writer.DeletedId);
    }

    private sealed class FakeSessionWriter : ISessionWriter
    {
        public SessionId? DeletedId { get; private set; }
        public Task RememberAsync(Cleo.Core.Domain.Entities.Session session, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task ForgetAsync(SessionId id, CancellationToken cancellationToken = default)
        {
            DeletedId = id;
            return Task.CompletedTask;
        }
    }
}
