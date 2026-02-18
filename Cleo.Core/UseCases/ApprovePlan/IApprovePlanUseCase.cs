using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.UseCases.ApprovePlan;

public record ApprovePlanRequest(SessionId Id, PlanId PlanId);

public record ApprovePlanResponse(SessionId Id, PlanId PlanId, DateTimeOffset ApprovedAt);

public interface IApprovePlanUseCase : IUseCase<ApprovePlanRequest, ApprovePlanResponse> { }
