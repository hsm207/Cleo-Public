using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.UseCases.RefreshPulse;

public record RefreshPulseRequest(SessionId Id);

public record RefreshPulseResponse(
    SessionId Id,
    SessionPulse Pulse,
    SessionState State,
    SessionActivity LastActivity,
    PullRequest? PullRequest = null,
    bool HasUnsubmittedSolution = false,
    bool IsCached = false,
    string? Warning = null
);

public interface IRefreshPulseUseCase : IUseCase<RefreshPulseRequest, RefreshPulseResponse> { }
