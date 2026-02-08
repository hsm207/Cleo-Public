using Cleo.Core.Domain.Ports;

namespace Cleo.Core.UseCases.ApprovePlan;

public class ApprovePlanUseCase : IApprovePlanUseCase
{
    private readonly ISessionController _controller;

    public ApprovePlanUseCase(ISessionController controller)
    {
        _controller = controller;
    }

    public async Task<ApprovePlanResponse> ExecuteAsync(ApprovePlanRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Formally approve the plan via the controller port
        await _controller.ApprovePlanAsync(request.Id, cancellationToken).ConfigureAwait(false);

        return new ApprovePlanResponse(request.Id, request.PlanId, DateTimeOffset.UtcNow);
    }
}
