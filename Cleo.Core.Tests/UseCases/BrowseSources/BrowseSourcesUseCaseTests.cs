using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.BrowseSources;
using Xunit;

namespace Cleo.Core.Tests.UseCases.BrowseSources;

public sealed class BrowseSourcesUseCaseTests
{
    private readonly FakeCatalog _catalog = new();
    private readonly BrowseSourcesUseCase _sut;

    public BrowseSourcesUseCaseTests()
    {
        _sut = new BrowseSourcesUseCase(_catalog);
    }

    [Fact(DisplayName = "When browsing sources, it should list all remote repository options.")]
    public async Task ShouldListSources()
    {
        // Arrange
        var source = new SessionSource("name", "owner", "repo");
        _catalog.Sources.Add(source);

        // Act
        var result = await _sut.ExecuteAsync(new BrowseSourcesRequest(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Sources);
        Assert.Equal(source, result.Sources[0]);
    }

    private sealed class FakeCatalog : ISourceCatalog
    {
        public List<SessionSource> Sources { get; } = new();
        public Task<IReadOnlyList<SessionSource>> GetAvailableSourcesAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<SessionSource>>(Sources);
    }
}
