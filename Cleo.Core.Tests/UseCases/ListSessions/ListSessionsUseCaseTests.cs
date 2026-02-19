using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.Tests.Builders;
using Cleo.Core.UseCases.ListSessions;
using Xunit;

namespace Cleo.Core.Tests.UseCases.ListSessions;

internal sealed class ListSessionsUseCaseTests
{
    private readonly FakeSessionReader _reader = new();
    private readonly ListSessionsUseCase _sut;

    public ListSessionsUseCaseTests()
    {
        _sut = new ListSessionsUseCase(_reader);
    }

    [Fact(DisplayName = "When listing sessions, it should return a summary of all active Sessions in the Task Registry.")]
    public async Task ShouldListAllSessions()
    {
        // Arrange
        var session = new SessionBuilder().Build();
        _reader.Sessions[session.Id] = session;

        // Act
        var result = await _sut.ExecuteAsync(new ListSessionsRequest(), TestContext.Current.CancellationToken).ConfigureAwait(false);

        // Assert
        Assert.Single(result.Sessions);
        Assert.Equal(session.Id, result.Sessions.First().Id);
    }

    private sealed class FakeSessionReader : ISessionReader
    {
        public Dictionary<SessionId, Session> Sessions { get; } = new();
        public Task<Session?> RecallAsync(SessionId id, CancellationToken cancellationToken = default) => Task.FromResult(Sessions.GetValueOrDefault(id));
        public Task<IReadOnlyCollection<Session>> ListAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Session>>(Sessions.Values.ToList());
    }
}
