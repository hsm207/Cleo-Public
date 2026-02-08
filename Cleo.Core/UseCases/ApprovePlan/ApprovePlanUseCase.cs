using Cleo.Core.Domain.Ports;

namespace Cleo.Core.UseCases.ApprovePlan;

public class ApprovePlanUseCase : IApprovePlanUseCase
{
    private readonly ISessionMessenger _messenger;

    public ApprovePlanUseCase(ISessionMessenger messenger)
    {
        _messenger = messenger;
    }

    public async Task<ApprovePlanResponse> ExecuteAsync(ApprovePlanRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Record a formal approval message
        var message = $"Plan {request.PlanId} approved.";
        await _messenger.SendMessageAsync(request.Id, message, cancellationToken).ConfigureAwait(false);

        return new ApprovePlanResponse(request.Id, request.PlanId, DateTimeOffset.UtcNow);
    }
}
