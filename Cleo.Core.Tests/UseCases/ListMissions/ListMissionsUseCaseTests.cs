using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.Tests.Builders;
using Cleo.Core.UseCases.ListMissions;
using Xunit;

namespace Cleo.Core.Tests.UseCases.ListMissions;

public sealed class ListMissionsUseCaseTests
{
    private readonly FakeSessionReader _reader = new();
    private readonly ListMissionsUseCase _sut;

    public ListMissionsUseCaseTests()
    {
        _sut = new ListMissionsUseCase(_reader);
    }

    [Fact(DisplayName = "When listing missions, it should return a summary of all active Sessions in the Task Registry.")]
    public async Task ShouldListAllMissions()
    {
        // Arrange
        var session = new SessionBuilder().Build();
        _reader.Sessions[session.Id] = session;

        // Act
        var result = await _sut.ExecuteAsync(new ListMissionsRequest(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Missions);
        Assert.Equal(session.Id, result.Missions.First().Id);
    }

    private sealed class FakeSessionReader : ISessionReader
    {
        public Dictionary<SessionId, Session> Sessions { get; } = new();
        public Task<Session?> GetByIdAsync(SessionId id, CancellationToken cancellationToken = default) => Task.FromResult(Sessions.GetValueOrDefault(id));
        public Task<IReadOnlyCollection<Session>> ListAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Session>>(Sessions.Values.ToList());
    }
}
