using Cleo.Core.Domain.Ports;

namespace Cleo.Core.UseCases.AbandonSession;

public class AbandonSessionUseCase : IAbandonSessionUseCase
{
    private readonly ISessionWriter _writer;

    public AbandonSessionUseCase(ISessionWriter writer)
    {
        _writer = writer;
    }

    public async Task<AbandonSessionResponse> ExecuteAsync(AbandonSessionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _writer.DeleteAsync(request.Id, cancellationToken).ConfigureAwait(false);
        return new AbandonSessionResponse(request.Id);
    }
}
