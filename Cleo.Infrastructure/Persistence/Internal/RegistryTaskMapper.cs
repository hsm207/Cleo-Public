using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Mappers;

namespace Cleo.Infrastructure.Persistence.Internal;

internal sealed class RegistryTaskMapper : IRegistryTaskMapper
{
    private readonly ActivityMapperFactory _activityFactory;

    public RegistryTaskMapper(ActivityMapperFactory activityFactory)
    {
        _activityFactory = activityFactory;
    }

    public SessionMetadataDto MapToMetadataDto(Session session)
    {
        var prDto = session.PullRequest != null
            ? new RegisteredPullRequestDto(
                session.PullRequest.Url,
                session.PullRequest.Title,
                session.PullRequest.Description,
                session.PullRequest.HeadRef,
                session.PullRequest.BaseRef)
            : null;

        return new SessionMetadataDto(
            session.Id.Value,
            session.RemoteId, // Fidelity!
            (string)session.Task,
            session.Source.Repository,
            session.Source.StartingBranch,
            session.Pulse.Status,
            session.DashboardUri,
            session.CreatedAt,
            session.UpdatedAt,
            prDto);
    }

    public Session MapFromMetadataDto(SessionMetadataDto dto)
    {
        // Enforce Fidelity: CreatedAt must be preserved.
        var createdAt = dto.CreatedAt;

        // Legacy Data Recovery: If CreatedAt is missing/default (0001-01-01), fallback to UpdatedAt or UtcNow.
        if (createdAt == default)
        {
             createdAt = dto.UpdatedAt ?? DateTimeOffset.UtcNow;
        }

        // Legacy Data Recovery: If RemoteId is missing, fallback to SessionId.
        var remoteId = !string.IsNullOrWhiteSpace(dto.RemoteId) ? dto.RemoteId : dto.SessionId;

        var session = new Session(
            new SessionId(dto.SessionId),
            remoteId,
            (TaskDescription)dto.TaskDescription,
            new SourceContext(dto.Repository, dto.SourceBranch),
            new SessionPulse(dto.PulseStatus),
            createdAt,
            dto.UpdatedAt,
            null,
            null,
            AutomationMode.Unspecified,
            dto.DashboardUri,
            new List<SessionActivity>()); // Empty history

        if (dto.PullRequest != null)
        {
            session.SetPullRequest(new PullRequest(
                dto.PullRequest.Url,
                dto.PullRequest.Title,
                dto.PullRequest.Description,
                dto.PullRequest.HeadRef,
                dto.PullRequest.BaseRef));
        }

        return session;
    }
}
