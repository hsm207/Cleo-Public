using Cleo.Core.Domain.Entities;

namespace Cleo.Core.UseCases.ListSessions;

public record ListSessionsRequest();

public record ListSessionsResponse(IReadOnlyCollection<Session> Sessions);

public interface IListSessionsUseCase : IUseCase<ListSessionsRequest, ListSessionsResponse> { }
