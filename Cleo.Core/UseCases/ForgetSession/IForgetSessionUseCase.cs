using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.UseCases.ForgetSession;

public record ForgetSessionRequest(SessionId Id);

public record ForgetSessionResponse(SessionId Id);

public interface IForgetSessionUseCase : IUseCase<ForgetSessionRequest, ForgetSessionResponse> { }
