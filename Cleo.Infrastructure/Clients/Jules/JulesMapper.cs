using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos;
using Cleo.Infrastructure.Clients.Jules.Mapping;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// Provides utility methods for mapping between Jules API Data Transfer Objects (DTOs) 
/// and core domain entities and value objects.
/// </summary>
internal static class JulesMapper
{
    public static Session Map(JulesSessionDto dto, TaskDescription originalTask, ISessionStatusMapper statusMapper)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(statusMapper);

        var status = statusMapper.Map(dto.State);
        
        return new Session(
            new SessionId(dto.Name),
            originalTask,
            new SourceContext(dto.SourceContext.Source, dto.SourceContext.GithubRepoContext?.StartingBranch ?? string.Empty),
            new SessionPulse(status, $"Session is {dto.State}")
        );
    }
}
