using Cleo.Core.Domain.Ports;

namespace Cleo.Core.UseCases.ListSessions;

public class ListSessionsUseCase : IListSessionsUseCase
{
    private readonly ISessionReader _reader;

    public ListSessionsUseCase(ISessionReader reader)
    {
        _reader = reader;
    }

    public async Task<ListSessionsResponse> ExecuteAsync(ListSessionsRequest request, CancellationToken cancellationToken = default)
    {
        var sessions = await _reader.ListAsync(cancellationToken).ConfigureAwait(false);
        return new ListSessionsResponse(sessions);
    }
}
