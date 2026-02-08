using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.UseCases.AbandonSession;

public record AbandonSessionRequest(SessionId Id);

public record AbandonSessionResponse(SessionId Id);

public interface IAbandonSessionUseCase : IUseCase<AbandonSessionRequest, AbandonSessionResponse> { }
