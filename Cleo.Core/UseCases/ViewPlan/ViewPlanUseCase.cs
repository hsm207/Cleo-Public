using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.UseCases.ViewPlan;

public class ViewPlanUseCase : IViewPlanUseCase
{
    private readonly ISessionReader _sessionReader;

    public ViewPlanUseCase(ISessionReader sessionReader)
    {
        _sessionReader = sessionReader;
    }

    public async Task<ViewPlanResponse> ExecuteAsync(ViewPlanRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var session = await _sessionReader.RecallAsync(request.SessionId, cancellationToken).ConfigureAwait(false);
        if (session == null)
        {
            return ViewPlanResponse.Empty();
        }

        var plan = session.GetLatestPlan();
        if (plan == null)
        {
            return ViewPlanResponse.Empty();
        }

        var steps = plan.Steps.Select(s => new PlanStepModel(s.Index, s.Title, s.Description)).ToList();

        return new ViewPlanResponse(true, plan.PlanId, plan.Timestamp, steps);
    }
}
