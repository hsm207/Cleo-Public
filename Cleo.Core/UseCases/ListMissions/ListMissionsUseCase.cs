using Cleo.Core.Domain.Ports;

namespace Cleo.Core.UseCases.ListMissions;

public class ListMissionsUseCase : IListMissionsUseCase
{
    private readonly ISessionReader _reader;

    public ListMissionsUseCase(ISessionReader reader)
    {
        _reader = reader;
    }

    public async Task<ListMissionsResponse> ExecuteAsync(ListMissionsRequest request, CancellationToken cancellationToken = default)
    {
        var missions = await _reader.ListAsync(cancellationToken).ConfigureAwait(false);
        return new ListMissionsResponse(missions);
    }
}
