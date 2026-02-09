using Cleo.Core.UseCases.ViewPlan;

namespace Cleo.Core.UseCases.ViewPlan;

public interface IViewPlanUseCase
{
    Task<ViewPlanResponse> ExecuteAsync(ViewPlanRequest request, CancellationToken cancellationToken = default);
}
