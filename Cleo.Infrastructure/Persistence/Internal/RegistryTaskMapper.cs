using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Mappers;

namespace Cleo.Infrastructure.Persistence.Internal;

internal sealed class RegistryTaskMapper : IRegistryTaskMapper
{
    private readonly ActivityMapperFactory _activityFactory;

    public RegistryTaskMapper(ActivityMapperFactory activityFactory)
    {
        _activityFactory = activityFactory ?? throw new ArgumentNullException(nameof(activityFactory));
    }

    public RegisteredSessionDto MapToDto(Session session) => new(
        session.Id.Value,
        (string)session.Task,
        session.Source.Repository,
        session.Source.StartingBranch,
        session.DashboardUri,
        session.SessionLog.Select(_activityFactory.ToEnvelope).ToList().AsReadOnly());

    public Session MapToDomain(RegisteredSessionDto dto)
    {
        var session = new Session(
            new SessionId(dto.SessionId),
            (TaskDescription)dto.TaskDescription,
            new SourceContext(dto.Repository, dto.Branch),
            new SessionPulse(SessionStatus.StartingUp), // Status remains ephemeral
            dto.DashboardUri);

        foreach (var envelope in dto.History ?? Enumerable.Empty<ActivityEnvelopeDto>())
        {
            session.AddActivity(_activityFactory.FromEnvelope(envelope));
        }

        return session;
    }
}
