using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.UseCases.ApprovePlan;

public record ApprovePlanRequest(SessionId Id, string PlanId);

public record ApprovePlanResponse(SessionId Id, string PlanId, DateTimeOffset ApprovedAt);

public interface IApprovePlanUseCase : IUseCase<ApprovePlanRequest, ApprovePlanResponse> { }
