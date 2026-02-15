using Cleo.Core.Domain.Ports;

namespace Cleo.Core.UseCases.BrowseHistory;

public class BrowseHistoryUseCase : IBrowseHistoryUseCase
{
    private readonly ISessionArchivist _archivist;
    private readonly ISessionReader _sessionReader;

    public BrowseHistoryUseCase(ISessionArchivist archivist, ISessionReader sessionReader)
    {
        _archivist = archivist;
        _sessionReader = sessionReader;
    }

    public async Task<BrowseHistoryResponse> ExecuteAsync(BrowseHistoryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var historyTask = _archivist.GetHistoryAsync(request.Id, cancellationToken);
        var sessionTask = _sessionReader.RecallAsync(request.Id, cancellationToken);

        await Task.WhenAll(historyTask, sessionTask).ConfigureAwait(false);

        var history = await historyTask.ConfigureAwait(false);
        var session = await sessionTask.ConfigureAwait(false);

        return new BrowseHistoryResponse(request.Id, history, session?.PullRequest);
    }
}
