namespace Cleo.Core.UseCases.ViewPlan;

public record PlanStepDto(int Index, string Title, string Description);

public record ViewPlanResponse(
    bool HasPlan,
    string? PlanId,
    DateTimeOffset? Timestamp,
    IReadOnlyList<PlanStepDto> Steps
)
{
    public static ViewPlanResponse Empty() => new(false, null, null, Array.Empty<PlanStepDto>());
}
