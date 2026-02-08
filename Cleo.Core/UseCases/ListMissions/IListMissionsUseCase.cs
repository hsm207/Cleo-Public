using Cleo.Core.Domain.Entities;

namespace Cleo.Core.UseCases.ListMissions;

public record ListMissionsRequest();

public record ListMissionsResponse(IReadOnlyCollection<Session> Missions);

public interface IListMissionsUseCase : IUseCase<ListMissionsRequest, ListMissionsResponse> { }
