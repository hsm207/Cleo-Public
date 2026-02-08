using Cleo.Core.Domain.Ports;

namespace Cleo.Core.UseCases.BrowseSources;

public class BrowseSourcesUseCase : IBrowseSourcesUseCase
{
    private readonly ISourceCatalog _catalog;

    public BrowseSourcesUseCase(ISourceCatalog catalog)
    {
        _catalog = catalog;
    }

    public async Task<BrowseSourcesResponse> ExecuteAsync(BrowseSourcesRequest request, CancellationToken cancellationToken = default)
    {
        var sources = await _catalog.GetAvailableSourcesAsync(cancellationToken).ConfigureAwait(false);
        return new BrowseSourcesResponse(sources);
    }
}
