using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.UseCases.RefreshPulse;

public record RefreshPulseRequest(SessionId Id);

public record RefreshPulseResponse(
    SessionId Id,
    SessionPulse Pulse,
    bool IsCached = false,
    string? Warning = null
);

public interface IRefreshPulseUseCase : IUseCase<RefreshPulseRequest, RefreshPulseResponse> { }
