using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.UseCases.BrowseHistory;

public record BrowseHistoryRequest(SessionId Id);

public record BrowseHistoryResponse(
    SessionId Id,
    IReadOnlyList<SessionActivity> History
);

public interface IBrowseHistoryUseCase : IUseCase<BrowseHistoryRequest, BrowseHistoryResponse> { }
