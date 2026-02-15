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

    public RegisteredSessionDto MapToDto(Session session) => new(
        session.Id.Value,
        (string)session.Task,
        session.Source.Repository,
        session.Source.StartingBranch,
        session.Pulse.Status,
        session.DashboardUri,
        session.SessionLog.Select(_activityFactory.ToEnvelope).ToList().AsReadOnly(),
        session.PullRequest?.Url,
        session.PullRequest?.Title,
        session.PullRequest?.Description);

    public Session MapToDomain(RegisteredSessionDto dto)
    {
        var history = dto.History?
            .Select(_activityFactory.FromEnvelope)
            .ToList();

        // Note: The registry is a simple store and might not have all the new metadata yet (e.g. AutomationMode).
        // We persist the PulseStatus to satisfy the High-Fidelity List requirement (RFC 015).
        var session = new Session(
            new SessionId(dto.SessionId),
            dto.SessionId, // Fallback RemoteId to SessionId for legacy persisted sessions
            (TaskDescription)dto.TaskDescription,
            new SourceContext(dto.Repository, dto.Branch),
            new SessionPulse(dto.PulseStatus), 
            DateTimeOffset.UtcNow, // Fallback for legacy persisted sessions
            null,
            null,
            null,
            AutomationMode.Unspecified,
            dto.DashboardUri,
            history);

        if (dto.PullRequestUrl != null && dto.PullRequestTitle != null)
        {
            session.SetPullRequest(new PullRequest(dto.PullRequestUrl, dto.PullRequestTitle, dto.PullRequestDescription));
        }

        return session;
    }
}
