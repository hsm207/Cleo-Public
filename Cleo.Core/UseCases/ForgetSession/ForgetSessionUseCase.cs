using Cleo.Core.Domain.Ports;

namespace Cleo.Core.UseCases.ForgetSession;

public class ForgetSessionUseCase : IForgetSessionUseCase
{
    private readonly ISessionWriter _writer;

    public ForgetSessionUseCase(ISessionWriter writer)
    {
        _writer = writer;
    }

    public async Task<ForgetSessionResponse> ExecuteAsync(ForgetSessionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _writer.ForgetAsync(request.Id, cancellationToken).ConfigureAwait(false);
        return new ForgetSessionResponse(request.Id);
    }
}
