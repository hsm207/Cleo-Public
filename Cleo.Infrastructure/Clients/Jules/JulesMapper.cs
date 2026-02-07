using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos;
using Cleo.Infrastructure.Clients.Jules.Mapping;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// A S.O.L.I.D. composite mapper that hydrates domain activities from Jules DTOs.
/// </summary>
internal static class JulesMapper
{
    private static readonly List<IJulesActivityMapper> Mappers = new()
    {
        new PlanningActivityMapper(),
        new ResultActivityMapper(),
        new ProgressActivityMapper(),
        new FailureActivityMapper(),
        new MessageActivityMapper() // Must be last as it's the catch-all! 🧤
    };

    public static SessionActivity Map(JulesActivityDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var mapper = Mappers.FirstOrDefault(m => m.CanMap(dto)) 
            ?? throw new InvalidOperationException($"No suitable mapper found for activity {dto.Id}.");

        return mapper.Map(dto);
    }
}
