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

    public RegisteredSessionDto MapToDto(Session session)
    {
        var prDto = session.PullRequest != null
            ? new RegisteredPullRequestDto(
                session.PullRequest.Url,
                session.PullRequest.Title,
                session.PullRequest.Description,
                session.PullRequest.HeadRef,
                session.PullRequest.BaseRef)
            : null;

        return new(
            session.Id.Value,
            (string)session.Task,
            session.Source.Repository,
            session.Source.StartingBranch,
            session.Pulse.Status,
            session.DashboardUri,
            session.SessionLog.Select(_activityFactory.ToEnvelope).ToList().AsReadOnly(),
            prDto);
    }

    public Session MapToDomain(RegisteredSessionDto dto)
    {
        var history = dto.History?
            .Select(_activityFactory.FromEnvelope)
            .ToList();

        var session = new Session(
            new SessionId(dto.SessionId),
            dto.SessionId,
            (TaskDescription)dto.TaskDescription,
            new SourceContext(dto.Repository, dto.SourceBranch),
            new SessionPulse(dto.PulseStatus),
            DateTimeOffset.UtcNow,
            null,
            null,
            null,
            AutomationMode.Unspecified,
            dto.DashboardUri,
            history);

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
