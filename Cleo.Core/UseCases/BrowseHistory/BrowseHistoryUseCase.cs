using Cleo.Core.Domain.Ports;

namespace Cleo.Core.UseCases.BrowseHistory;

public class BrowseHistoryUseCase : IBrowseHistoryUseCase
{
    private readonly ISessionArchivist _archivist;

    public BrowseHistoryUseCase(ISessionArchivist archivist)
    {
        _archivist = archivist;
    }

    public async Task<BrowseHistoryResponse> ExecuteAsync(BrowseHistoryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var history = await _archivist.GetHistoryAsync(request.Id, cancellationToken).ConfigureAwait(false);
        return new BrowseHistoryResponse(request.Id, history);
    }
}
