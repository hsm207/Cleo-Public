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
        session.DashboardUri,
        session.SessionLog.Select(_activityFactory.ToEnvelope).ToList().AsReadOnly(),
        session.PullRequest?.Url,
        session.PullRequest?.Title,
        session.PullRequest?.Description);

    public Session MapToDomain(RegisteredSessionDto dto)
    {
        // Note: The registry is a simple store and might not have all the new metadata yet.
        // We'll use defaults for now, but this highlights a potential gap in persistence if we want to store these fields.
        // However, the registry seems to be local cache/history.
        var session = new Session(
            new SessionId(dto.SessionId),
            dto.SessionId, // Fallback RemoteId to SessionId for legacy persisted sessions
            (TaskDescription)dto.TaskDescription,
            new SourceContext(dto.Repository, dto.Branch),
            new SessionPulse(SessionStatus.StartingUp), // Status remains ephemeral
            DateTimeOffset.UtcNow, // Fallback for legacy persisted sessions
            null,
            null,
            null,
            AutomationMode.Unspecified,
            dto.DashboardUri);

        foreach (var envelope in dto.History ?? Enumerable.Empty<ActivityEnvelopeDto>())
        {
            session.AddActivity(_activityFactory.FromEnvelope(envelope));
        }

        if (dto.PullRequestUrl != null && dto.PullRequestTitle != null)
        {
            session.SetPullRequest(new PullRequest(dto.PullRequestUrl, dto.PullRequestTitle, dto.PullRequestDescription));
        }

        return session;
    }
}
