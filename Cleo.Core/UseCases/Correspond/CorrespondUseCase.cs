using Cleo.Core.Domain.Ports;

namespace Cleo.Core.UseCases.Correspond;

public class CorrespondUseCase : ICorrespondUseCase
{
    private readonly ISessionMessenger _messenger;
    private readonly ISessionReader _reader;

    public CorrespondUseCase(ISessionMessenger messenger, ISessionReader reader)
    {
        _messenger = messenger;
        _reader = reader;
    }

    public async Task<CorrespondResponse> ExecuteAsync(CorrespondRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var session = await _reader.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (session == null)
        {
            throw new InvalidOperationException($"🔍 Handle {request.Id} not found in the registry. 🥀");
        }

        await _messenger.SendMessageAsync(request.Id, request.Message, cancellationToken).ConfigureAwait(false);

        return new CorrespondResponse(request.Id, DateTimeOffset.UtcNow);
    }
}
