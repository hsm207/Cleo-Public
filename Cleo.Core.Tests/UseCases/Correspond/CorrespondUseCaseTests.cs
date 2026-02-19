using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.Correspond;
using Cleo.Tests.Common;
using Xunit;
using Cleo.Core.Tests.Builders;

namespace Cleo.Core.Tests.UseCases.Correspond;

internal sealed class CorrespondUseCaseTests
{
    private readonly FakeMessenger _messenger = new();
    private readonly FakeSessionReader _reader = new();
    private readonly CorrespondUseCase _sut;

    public CorrespondUseCaseTests()
    {
        _sut = new CorrespondUseCase(_messenger, _reader);
    }

    [Fact(DisplayName = "When sending a message to a Session, then it should be recorded as Developer Feedback.")]
    public async Task ShouldSendMessage()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("123");
        var session = new SessionBuilder().WithId(sessionId.Value).Build();
        _reader.Sessions[sessionId] = session;
        var request = new CorrespondRequest(sessionId, "Hello");

        // Act
        var result = await _sut.ExecuteAsync(request, TestContext.Current.CancellationToken).ConfigureAwait(false);

        // Assert
        Assert.Equal(sessionId, result.Id);
        Assert.Equal("Hello", _messenger.LastMessage);
    }

    [Fact(DisplayName = "Given a Handle that does not exist, when sending a message, then it should notify that the session is unknown.")]
    public async Task ShouldThrowWhenHandleNotFound()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("ghost");
        var request = new CorrespondRequest(sessionId, "Hi");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ExecuteAsync(request, TestContext.Current.CancellationToken)).ConfigureAwait(false);
    }

    private sealed class FakeMessenger : ISessionMessenger
    {
        public string? LastMessage { get; private set; }
        public Task SendMessageAsync(SessionId id, string message, CancellationToken cancellationToken = default)
        {
            LastMessage = message;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSessionReader : ISessionReader
    {
        public Dictionary<SessionId, Cleo.Core.Domain.Entities.Session> Sessions { get; } = new();
        public Task<Cleo.Core.Domain.Entities.Session?> RecallAsync(SessionId id, CancellationToken cancellationToken = default) => Task.FromResult(Sessions.GetValueOrDefault(id));
        public Task<IReadOnlyCollection<Cleo.Core.Domain.Entities.Session>> ListAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
