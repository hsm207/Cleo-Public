namespace Cleo.Core.UseCases.ViewPlan;

public record PlanStepModel(int Index, string Title, string Description);

public record ViewPlanResponse(
    bool HasPlan,
    string? PlanId,
    DateTimeOffset? Timestamp,
    IReadOnlyList<PlanStepModel> Steps,
    bool IsApproved
)
{
    public static ViewPlanResponse Empty() => new(false, null, null, Array.Empty<PlanStepModel>(), false);
}
